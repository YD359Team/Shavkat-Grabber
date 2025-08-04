using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using DynamicData;
using ReactiveUI;
using Shavkat_grabber.Helpers;
using Shavkat_grabber.Logic;
using Shavkat_grabber.Logic.Abstract;
using Shavkat_grabber.Logic.Db;
using Shavkat_grabber.Models;
using Shavkat_grabber.Models.Tree;
using Shavkat_grabber.Views;

namespace Shavkat_grabber.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly FileSystemManager _fsManager;
    private readonly WindowManager _winManager;
    private AppSettings _settings;
    private readonly AppStartParams _appStartParams;

    public List<TreeViewNode> Root { get; private set; }
    ObservableCollection<ProductChecked> _items = new();
    public FlatTreeDataGridSource<ProductChecked> ItemsSource { get; }

    private int _dataToLoadCount = 2;

    /// <summary>
    /// Загружены для все данные для работы программы
    /// </summary>
    public bool IsAllDataLoaded
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Запущено ли в данный момент сканирование
    /// </summary>
    public bool IsLoading
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    private int _selectedCount = 1;

    /// <summary>
    /// Сколько товаров выбранных категорий сканировать
    /// </summary>
    public int SelectedCount
    {
        get => _selectedCount;
        set => this.RaiseAndSetIfChanged(ref _selectedCount, value);
    }

    private int _checkedCount;

    /// <summary>
    /// Сколько ключевых слов для сканирования выбрано
    /// </summary>
    public int CheckedCount
    {
        get => _checkedCount;
        set => this.RaiseAndSetIfChanged(ref _checkedCount, value);
    }

    public ObservableCollection<LogMessage> LogMessages { get; } = new();

    public MainWindowViewModel() { }

    public MainWindowViewModel(MainWindow mainWindow, AppStartParams appStartParams)
    {
        LoadSettingsOrCreateAsync();
        LoadGroupsAndKeywordsAsync();

        ItemsSource = new FlatTreeDataGridSource<ProductChecked>(_items)
        {
            Columns =
            {
                new CheckBoxColumn<ProductChecked>(
                    "✅",
                    x => x.IsChecked,
                    (x, o) =>
                    {
                        x.IsChecked = o;
                        if (o)
                            CheckedCount++;
                        else
                            CheckedCount--;
                    }
                ),
                new TextColumn<ProductChecked, string>("Артикул", x => x.Good.Article),
                new TextColumn<ProductChecked, string>("Название", x => x.Good.Title),
                new TextColumn<ProductChecked, string>("Цена", x => x.Good.Price),
                new TextColumn<ProductChecked, string>("URL", x => x.Good.Url),
                new TextColumn<ProductChecked, string>("Фото", x => x.Good.ImageUrl),
            },
        };

        _fsManager = new();
        _winManager = new(mainWindow);
        _appStartParams = appStartParams;

#if DEBUG
        ProductChecked[] items =
        [
            new(DebugHelper.GetTestGood()) { IsChecked = true },
            new(DebugHelper.GetTestGood()) { IsChecked = true },
        ];
        _items.AddRange(items);
        CheckedCount += items.Length;
        LogMessages.Add(new LogMessage("Загружены тестовые товары (2)", LogMessageTypes.Success));
#endif
    }

    private async Task LoadSettingsOrCreateAsync()
    {
        if (!File.Exists("settings.json"))
        {
            _settings = AppSettings.CreateDefault();
            return;
        }

        string json = await File.ReadAllTextAsync("settings.json");
        _settings = SerializeHelper.Deserialize<AppSettings>(json);

        if (Interlocked.Decrement(ref _dataToLoadCount) == 0)
        {
            IsAllDataLoaded = true;
        }
    }

    private async Task LoadGroupsAndKeywordsAsync()
    {
        Root = new(await new DbManager().LoadGroupsOrCreateIfNotExist());
        this.RaisePropertyChanged(nameof(Root));

        if (Interlocked.Decrement(ref _dataToLoadCount) == 0)
        {
            IsAllDataLoaded = true;
        }
    }

    public async Task Start()
    {
        IsLoading = true;
        string[] keyWords = Root.OfType<TreeViewNodeGroup>()
            .SelectMany(x => x.Children)
            .Where(x => x.IsChecked)
            .Select(x => x.Title)
            .ToArray();
        if (keyWords.Length == 0)
        {
            IsLoading = false;
            return;
        }

        using WbDriver driver = await DriverBase.CreateAsync<WbDriver>(_fsManager, _settings);
        driver.OnLogMessage += OnLogMessage;
        driver.OnScaningEnd += OnDriverOnOnScaningEnd;
        await foreach (var item in driver.StartGrab(keyWords, SelectedCount))
        {
            _items.Add(new ProductChecked(item) { IsChecked = true });
            CheckedCount++;
        }
        driver.OnLogMessage -= OnLogMessage;
        driver.OnScaningEnd -= OnDriverOnOnScaningEnd;
        IsLoading = false;
    }

    private void OnDriverOnOnScaningEnd(object? s, EventArgs e)
    {
        IsLoading = false;
    }

    private void OnLogMessage(LogMessage logMessage)
    {
        LogMessages.Add(logMessage);
        Dispatcher.UIThread.Post(
            x =>
            {
                _winManager.MainWindow.ScrollListBox(logMessage);
            },
            null
        );
    }

    public async Task Export(object eFormat)
    {
        string formatStr = SerializeHelper.Serialize(eFormat);
        var format = Enum.Parse<WindowManager.FileFormats>(formatStr.ToUpperInvariant());
        string? path = await _winManager.SaveFileDialog(format);

        if (path is null)
        {
            return;
        }

        Exporter export = new();
        if (formatStr == "csv")
        {
            await export.ToCsv(_items.Where(x => x.IsChecked).Select(x => x.Good), path);
            ShowSuccess("Сохранено");
        }
        else if (formatStr == "xlsx")
        {
            await export.ToXlsx(_items.Where(x => x.IsChecked).Select(x => x.Good), path);
            ShowSuccess("Сохранено");
        }
    }

    public async void CreatePost()
    {
        Product[] goods = _items.Where(x => x.IsChecked).Select(x => x.Good).ToArray();

        PostingWindow tgPostWindow = new();
        tgPostWindow.DataContext = new PostingWindowViewModel(
            _fsManager,
            _winManager,
            _settings,
            goods
        );
        await tgPostWindow.ShowDialog(_winManager.MainWindow);
    }

    public async void ShowHelp()
    {
        HelpWindow settingsWindow = new();
        settingsWindow.DataContext = new HelpWindowViewModel(_fsManager, _winManager, _settings);
        await settingsWindow.ShowDialog(_winManager.MainWindow);
    }

    public async void ShowSettings()
    {
        SettingsWindow settingsWindow = new();
        settingsWindow.DataContext = new SettingsWindowViewMode(_fsManager, _winManager, _settings);

        AppSettings? newSettings = await settingsWindow.ShowDialog<AppSettings>(
            _winManager.MainWindow
        );
        if (newSettings is null)
            return;

        _settings = newSettings;
        string json = SerializeHelper.Serialize(_settings);
        await File.WriteAllTextAsync("settings.json", json);

        ShowSuccess("Настройки сохранены");
    }

    public void ShowSuccess(string msg)
    {
        _winManager.MainWindow.ShowNotification("Успех", msg, NotificationType.Success);
    }

    public void ShowError(string msg)
    {
        _winManager.MainWindow.ShowNotification("Ошибка", msg, NotificationType.Error);
    }

    public void ConsoleClear()
    {
        LogMessages.Clear();
    }

    public async void ShowDatabaseWindow()
    {
        StatisticsWindow databaseWindow = new();
        databaseWindow.DataContext = new StatisticsWindowViewModel(
            _fsManager,
            _winManager,
            _settings
        );
        await databaseWindow.ShowDialog(this._winManager.MainWindow);
    }

    public async void OnAddGroupClick(object eText)
    {
        string? gpName = eText?.ToString();
        if (gpName is null)
            return;

        var result = await _winManager.Question(
            $"Добавить группу \"{gpName}\"?",
            DialogResultButtons.Yes | DialogResultButtons.Cancel
        );

        if (result != DialogResultButtons.Yes)
            return;

        var gpNode = new TreeViewNodeGroup(gpName);
        Root.Add(gpNode);
        Group group = new(gpName);
        int id = await new DbManager().AddGroupAsync(group);
        gpNode.Id = id;
        Root.Add(gpNode);
    }

    public async void OnAddKeywordClick(object eGroupNode)
    {
        var kwResult = await _winManager.ShowEditForm(WindowManager.EditMode.Add);
        if (!kwResult.IsSuccess)
            return;

        TreeViewNodeGroup gpNode = (TreeViewNodeGroup)eGroupNode;
        DbManager db = new DbManager();
        Group? gp = await db.FindGroupById(gpNode.Id);

        Keyword kw = new(0, gp.Id, kwResult.Value);
        int id = await db.AddKeywordAsync(kw);

        TreeViewNodeItem kwNode = new(id, kwResult.Value);
        gpNode.Children.Add(kwNode);
    }
}
