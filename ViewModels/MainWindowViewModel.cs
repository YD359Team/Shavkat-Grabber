using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using ReactiveUI;
using Shavkat_grabber.Helpers;
using Shavkat_grabber.Logic;
using Shavkat_grabber.Models;
using Shavkat_grabber.Views;

namespace Shavkat_grabber.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private WindowManager _manager;
    private AppSettings _settings;

    public List<TreeViewNode> Root { get; } =
        [
            new TreeViewNodeGroup(
                "Для неё",
                new TreeViewNodeItem("Ароматические свечи"),
                new TreeViewNodeItem("Шоколад ручной работы"),
                new TreeViewNodeItem("Бомбочки для ванны"),
                new TreeViewNodeItem("Скраб/ Крем для тела"),
                new TreeViewNodeItem("Крем для рук"),
                new TreeViewNodeItem("Чашка"),
                new TreeViewNodeItem("Чай"),
                new TreeViewNodeItem("Мини-заварник для чая"),
                new TreeViewNodeItem("Средства для волос"),
                new TreeViewNodeItem("Маски для лица"),
                new TreeViewNodeItem("Мыло ручной работы"),
                new TreeViewNodeItem("Косметичка"),
                new TreeViewNodeItem("Расческа"),
                new TreeViewNodeItem("Шелковые наволочки"),
                new TreeViewNodeItem("Заколки"),
                new TreeViewNodeItem("Ободки / повязки на голову для умывания"),
                new TreeViewNodeItem("Соль дня ванной"),
                new TreeViewNodeItem("Арома-масла"),
                new TreeViewNodeItem("Патчи для глаз"),
                new TreeViewNodeItem("Тапочки")
            ),
            new TreeViewNodeGroup(
                "Для него",
                new TreeViewNodeItem("Кружки / термостаканы"),
                new TreeViewNodeItem("Чай"),
                new TreeViewNodeItem("Шоколад"),
                new TreeViewNodeItem("Техника (например, беспроводное зарядное устройство)"),
                new TreeViewNodeItem("Аксессуары для компьютера"),
                new TreeViewNodeItem("Аксессуары в авто"),
                new TreeViewNodeItem("Мужские украшения"),
                new TreeViewNodeItem("Мужская косметика"),
                new TreeViewNodeItem("Галстук/ галстук-бабочка"),
                new TreeViewNodeItem("Настольная игра")
            ),
            new TreeViewNodeGroup(
                "Для дома",
                new TreeViewNodeItem("Постеры"),
                new TreeViewNodeItem("Ковер"),
                new TreeViewNodeItem("Ароматы для дома"),
                new TreeViewNodeItem("Керамика"),
                new TreeViewNodeItem("Гирлянда")
            ),
        ];

    ObservableCollection<GoodItem> _items = new();
    public FlatTreeDataGridSource<GoodItem> Items { get; }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set => this.RaiseAndSetIfChanged(ref _isLoading, value);
    }

    private int _selectedCount = 1;
    public int SelectedCount
    {
        get => _selectedCount;
        set => this.RaiseAndSetIfChanged(ref _selectedCount, value);
    }

    private int _checkedCount;
    public int CheckedCount
    {
        get => _checkedCount;
        set => this.RaiseAndSetIfChanged(ref _checkedCount, value);
    }

    public ObservableCollection<LogMessage> LogMessages { get; } = new();

    private readonly FilePickerFileType[] fileTypesCsv = [new("CSV") { Patterns = ["*.csv"] }];
    private readonly FilePickerFileType[] fileTypesXlsx = [new("XLSX") { Patterns = ["*.xlsx"] }];

    public MainWindowViewModel() { }

    public MainWindowViewModel(MainWindow mainWindow)
    {
        LoadSettingsOrCreate();

        Items = new FlatTreeDataGridSource<GoodItem>(_items)
        {
            Columns =
            {
                new CheckBoxColumn<GoodItem>(
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
                new TextColumn<GoodItem, string>("Артикул", x => x.Good.Article),
                new TextColumn<GoodItem, string>("Название", x => x.Good.Title),
                new TextColumn<GoodItem, string>("Производитель", x => x.Good.Vendor),
                new TextColumn<GoodItem, string>("Цена", x => x.Good.Price),
                new TextColumn<GoodItem, string>("Старая цена", x => x.Good.OldPrice),
                new TextColumn<GoodItem, string>("URL", x => x.Good.Url),
                new TextColumn<GoodItem, string>("Фото", x => x.Good.ImageUrl),
                new TextColumn<GoodItem, string>("Рейтинг", x => x.Good.Rating),
            },
        };

        _manager = new(mainWindow);
    }

    private void LoadSettingsOrCreate()
    {
        if (!File.Exists("settings.json"))
        {
            _settings = AppSettings.CreateDefault();
            return;
        }

        string json = File.ReadAllText("settings.json");
        _settings = SerializeHelper.Deserialize<AppSettings>(json);
    }

    public async void Start()
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

        Driver driver = await Driver.CreateAsync(_settings);
        driver.OnLogMessage += OnLogMessage;
        driver.OnScaningEnd += OnDriverOnOnScaningEnd;
        await foreach (var item in driver.StartGrab(keyWords, SelectedCount))
        {
            _items.Add(new GoodItem(item) { IsChecked = true });
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
                _manager.MainWindow.ScrollListBox(logMessage);
            },
            null
        );
    }

    public async void Export(object eFormat)
    {
        string format = eFormat.ToString();
        var topLevel = TopLevel.GetTopLevel(_manager.MainWindow);

        var options = new FilePickerSaveOptions();
        if (format == "csv")
        {
            options.FileTypeChoices = fileTypesCsv;
        }
        else if (format == "xlsx")
        {
            options.FileTypeChoices = fileTypesXlsx;
        }
        else
        {
            throw new NotImplementedException();
        }

        using var file = await topLevel.StorageProvider.SaveFilePickerAsync(options);
        if (file is null)
            return;

        Exporter export = new();
        if (format == "csv")
        {
            await export.ToCsv(
                _items.Where(x => x.IsChecked).Select(x => x.Good),
                await file.OpenWriteAsync()
            );
            ShowSuccess("Сохранено");
        }
        else if (format == "xlsx")
        {
            await export.ToXlsx(
                _items.Where(x => x.IsChecked).Select(x => x.Good),
                file.Path.AbsolutePath
            );
            ShowSuccess("Сохранено");
        }
    }

    public async void CreatePost()
    {
        Good[] goods = _items.Where(x => x.IsChecked).Select(x => x.Good).ToArray();

        TelegramPostWindow tgPostWindow = new();
        tgPostWindow.DataContext = new TelegramPostWindowViewMode(_manager, _settings, goods);
        await tgPostWindow.ShowDialog(_manager.MainWindow);
    }

    public async void ShowSettings()
    {
        SettingsWindow settingsWindow = new();
        settingsWindow.DataContext = new SettingsWindowViewMode(_manager, _settings);

        AppSettings? newSettings = await settingsWindow.ShowDialog<AppSettings>(
            _manager.MainWindow
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
        _manager.MainWindow.ShowNotification("Успех", msg, NotificationType.Success);
    }
}
