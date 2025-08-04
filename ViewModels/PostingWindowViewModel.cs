using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using DynamicData;
using ReactiveUI;
using Shavkat_grabber.Extensions;
using Shavkat_grabber.Logic;
using Shavkat_grabber.Logic.Abstract;
using Shavkat_grabber.Logic.API;
using Shavkat_grabber.Logic.Pattern;
using Shavkat_grabber.Models;
using Shavkat_grabber.Models.Json;
using Shavkat_grabber.Views;
using YDs_DLL_BASE.Extensions;

namespace Shavkat_grabber.ViewModels;

public class PostingWindowViewModel : ChildViewModel
{
    private readonly GigaChatApi _gigaChatApi;
    private readonly TelegramBotApi _telegram;

    private readonly Product[] _goods;

    public bool IsAsyncDataLoaded
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsGigaChatApiConnected
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsTgBotConnected
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public int ImageLoadedCount
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool IsImagesLoaded
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public Bitmap Collage
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string PostText
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool HasCriticalError
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool CompletePostTelegram
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool CompletePostPinterest
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public bool CompletePostTiktok
    {
        get => field;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ObservableCollection<PreviewImage> Attachments { get; } = new();

    public PostingWindowViewModel()
        : base(null, null, null) { }

    public PostingWindowViewModel(
        FileSystemManager fsManager,
        WindowManager manager,
        AppSettings appSettings,
        Product[] goods
    )
        : base(fsManager, manager, appSettings)
    {
        _goods = goods;
        _gigaChatApi = new(appSettings.GigaChatAuthKey, appSettings.GigaChatScope);
        _telegram = new(appSettings.TgBotToken, appSettings.TgChannelId);

        InitAsyncData();
    }

    private async Task InitAsyncData()
    {
        IsGigaChatApiConnected = true;

        Task tgBotTask = TgBotTask();
        if (HasCriticalError)
            return;

        Task getImages = GetImagesFromGoods();

        await Task.WhenAll(tgBotTask, getImages);

        while (ImageLoadedCount < _goods.Length)
        {
            await Task.Delay(200);
        }

        IsImagesLoaded = true;

        // коллаж
        CreateCollage();

        // текст
        await CreateText();
        if (HasCriticalError)
            return;

        IsAsyncDataLoaded = true;
    }

    private async Task CreateText()
    {
        StringBuilder builder = new();
        builder.AppendLine();
        for (var index = 0; index < _goods.Length; index++)
        {
            var good = _goods[index];
            builder.Append(good.Title);
            if (index + 1 < _goods.Length)
                builder.Append("; ");
        }
        var result = await _gigaChatApi.GetTextResultAsync(
            "Составь одно короткое описание этих товаров для поста без форматирования: " + builder
        );

        if (!result.IsSuccess)
        {
            HasCriticalError = true;
            await WinManager.ShowError(result.Error);
            return;
        }

        PostText = string.Join("\n", [Settings.StaticHeader, result.Value, Settings.StaticFooter]);
    }

    private void CreateCollage()
    {
        DrawingController drawingController = new();
        List<ImageWithArticle> images = new();
        for (var index = 0; index < _goods.Length; index++)
        {
            var good = _goods[index];
            var attach = Attachments.FirstOrDefault(x => x.Id == index + 1);
            if (attach is null)
                continue;

            images.Add(new ImageWithArticle(good.Article, attach.Image));
        }

        Collage = drawingController.CreateCollage("WB", images);
    }

    private async Task TgBotTask()
    {
        var result = await _telegram.CheckConnection();
        IsTgBotConnected = result.IsSuccess;

        if (!IsTgBotConnected)
        {
            HasCriticalError = true;
            await WinManager.ShowError(result.Error);
        }
    }

    private async Task GetImagesFromGoods()
    {
        Attachments.AddRange(_goods.Select((x, idx) => new PreviewImage(idx + 1, null)));

        int index = 1;
        foreach (var good in _goods)
        {
            await DownloadImage(good.ImageUrl, index);
            index++;
        }
    }

    private void DownloadComplete(object sender, DownloadDataCompletedEventArgs e)
    {
        try
        {
            int index = (int)e.UserState;
            byte[] bytes = e.Result;
            using Stream stream = new MemoryStream(bytes);
            var image = new Bitmap(stream);

            // Заменяем существующий элемент (чтобы не нарушать порядок)
            Attachments[index - 1] = new PreviewImage(index, image);
            ImageLoadedCount++;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
        }
    }

    public async Task DownloadImage(string url, int i)
    {
        using WebClient client = new WebClient();
        client.DownloadDataAsync(new Uri(url), i);
        client.DownloadDataCompleted += DownloadComplete;
    }

    public async void Post()
    {
        try
        {
            IsAsyncDataLoaded = false;
            TextController textController = new();
#if DEBUG
            int tgPostId = 84;
            CompletePostTelegram = true;
#else

            Result<int> result = await _telegram.Post(
                textController.AddMarkdownLinks(PostText, _goods),
                Attachments.Select(x => x.Image).ToArray()
            );
            CompletePostTelegram = result.IsSuccess;

            int tgPostId = result.Value;
#endif

            string otherPostText = textController.AddLinkToTelegram(PostText, Settings, tgPostId);

            if (CompletePostTelegram)
            {
                string collagePath = FsManager.SaveBitmapInTempAndGetFullPath(Collage);
                PinterestDriver driver = new PinterestDriver();
                await driver.InitializeAsync(Settings);
                await driver.LoginAsync(Settings.PinterestEmail, Settings.PinterestPassword);
                await driver.CreatePinWithUniversalSelectorsAsync(
                    collagePath,
                    "Тестовый заголовок",
                    otherPostText
                );
                await driver.CloseAsync();
            }
        }
        catch (Exception ex)
        {
            await WinManager.ShowError(ex);
        }
        finally
        {
            IsAsyncDataLoaded = true;
        }
    }

    public async void RemoveBgAttachAll()
    {
        try
        {
            IsAsyncDataLoaded = false;

            foreach (var attach in Attachments)
            {
                Bitmap bmp = attach.Image;
                var (path, targetPath) = SaveBitmap(bmp);

                string args = Settings.RemoveBgColor.IsNullOrWhiteSpace()
                    ? $"--api-key {Settings.RemoveBgApiKey} \"{path}\""
                    : $"--api-key {Settings.RemoveBgApiKey} \"{path}\" --bg-color {Settings.RemoveBgColor}";
                await FsManager.StartProcessAndWait(Settings.RemoveBgCliPath, args);
                await Task.Delay(250);

                attach.Image = new Bitmap(targetPath);
                attach.RaisePropertyChanged(nameof(attach.Image));
            }

            CreateCollage();
        }
        catch (Exception ex)
        {
            await WinManager.ShowError(ex);
        }
        finally
        {
            IsAsyncDataLoaded = true;
        }
    }

    public async void RemoveBgAttach(object e)
    {
        PreviewImage prev = (PreviewImage)e;
        Bitmap bmp = prev.Image;
        try
        {
            IsAsyncDataLoaded = false;

            var (path, targetPath) = SaveBitmap(bmp);

            string args = string.IsNullOrWhiteSpace(Settings.RemoveBgColor)
                ? $"--api-key {Settings.RemoveBgApiKey} \"{path}\""
                : $"--api-key {Settings.RemoveBgApiKey} \"{path}\" --bg-color {Settings.RemoveBgColor}";
            await FsManager.StartProcessAndWait(Settings.RemoveBgCliPath, args);
            await Task.Delay(250);

            prev.Image = new Bitmap(targetPath);
            prev.RaisePropertyChanged(nameof(prev.Image));
            CreateCollage();
        }
        catch (Exception ex)
        {
            await WinManager.ShowError(ex);
        }
        finally
        {
            IsAsyncDataLoaded = true;
        }
    }

    private (string baseName, string targetName) SaveBitmap(Bitmap attachment)
    {
        string path = Path.Combine(Environment.CurrentDirectory, "Temp");
        FsManager.DirClearOrCreateIfNotExist(path);

        string baseName = Path.Combine(path, "img.png");
        string targetName = Path.Combine(path, "img-removebg.png");
        attachment.Save(baseName);

        return (baseName, targetName);
    }

    public async void SaveAllPreview()
    {
        try
        {
            foreach (var attach in Attachments)
            {
                string? path = await WinManager.SaveFileDialog(WindowManager.FileFormats.Png);
                if (path is null)
                    continue;
                attach.Image.Save(path);
            }
        }
        catch (Exception ex)
        {
            await WinManager.ShowError(ex);
        }
    }
}
