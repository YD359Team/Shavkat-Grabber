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
using Shavkat_grabber.Models;
using Shavkat_grabber.Models.Json;
using Shavkat_grabber.Views;

namespace Shavkat_grabber.ViewModels;

public class PostingWindowViewModel : ChildViewModel
{
    private readonly Logic.GigaChatApi _gigaChatApi;
    private readonly Logic.TelegramBotApi _telegram;

    private Good[] _goods;

    private bool _isAsyncDataLoaded;
    public bool IsAsyncDataLoaded
    {
        get => _isAsyncDataLoaded;
        set => this.RaiseAndSetIfChanged(ref _isAsyncDataLoaded, value);
    }

    private bool _isGigaChatApiConnected;
    public bool IsGigaChatApiConnected
    {
        get => _isGigaChatApiConnected;
        set => this.RaiseAndSetIfChanged(ref _isGigaChatApiConnected, value);
    }

    private bool _isTgBotConnected;
    public bool IsTgBotConnected
    {
        get => _isTgBotConnected;
        set => this.RaiseAndSetIfChanged(ref _isTgBotConnected, value);
    }

    private int _imageLoaded;
    public int ImageLoaded
    {
        get => _imageLoaded;
        set => this.RaiseAndSetIfChanged(ref _imageLoaded, value);
    }

    private string _postText;
    public string PostText
    {
        get => _postText;
        set => this.RaiseAndSetIfChanged(ref this._postText, value);
    }

    private bool _postTelegram = false;
    public bool PostTelegram
    {
        get => _postTelegram;
        set => this.RaiseAndSetIfChanged(ref _postTelegram, value);
    }

    private bool _postPinterest = true;
    public bool PostPinterest
    {
        get => _postPinterest;
        set => this.RaiseAndSetIfChanged(ref _postPinterest, value);
    }

    private bool _postTiktok = false;
    public bool PostTiktok
    {
        get => _postTiktok;
        set => this.RaiseAndSetIfChanged(ref _postTiktok, value);
    }

    public ObservableCollection<PreviewImage> Attachments { get; } = new();

    public PostingWindowViewModel()
        : base(null, null, null) { }

    public PostingWindowViewModel(FileSystemManager fsManager, WindowManager manager, AppSettings appSettings, Good[] goods)
        : base(fsManager, manager, appSettings)
    {
        _goods = goods;
        _gigaChatApi = new(appSettings.GigaChatAuthKey, appSettings.GigaChatScope);
        _telegram = new(appSettings.TgBotToken, appSettings.TgChannelId);

        InitAsyncData();
    }

    private async Task InitAsyncData()
    {
        //Task gigaChatApiTask = GigaChatApiTask();
        Task tgBotTask = TgBotTask();
        Task getImages = GetImagesFromGoods();

        await Task.WhenAll( /*gigaChatApiTask*/
            tgBotTask,
            getImages
        );

        Attachments.Sort();

        PostText = await GetTextFromGoods();

        IsAsyncDataLoaded = true;
    }

    private async Task TgBotTask()
    {
        IsTgBotConnected = await _telegram.CheckConnection();
    }

    private async Task<string> GetTextFromGoods()
    {
        bool isSingleGood = _goods.Length == 1;

        StringBuilder sb = new();

        if (!string.IsNullOrWhiteSpace(Settings.StaticHeader))
        {
            sb.AppendLine(Settings.StaticHeader);
            sb.AppendLine();
        }

/*IsGigaChatApiConnected*/
        if (
            #if DEBUG
            false 
            #elif RELEASE
            true
            #endif
        )
        {
            string quest =
                Settings.GigaChatPrompt + " " + string.Join(';', _goods.Select(x => x.Title));
            Console.WriteLine("gigachat question...");
            Result<AnswerRoot> answer = await _gigaChatApi.SendMessage(quest);
            Console.WriteLine("gigachat anser");

            if (answer.IsSuccess)
            {
                sb.AppendLine(
                    answer
                        .Value.choices.First()
                        .message.content.Replace(@"\n", "\n")
                        .Replace("**", "*")
                        .Replace("__", "_")
                        .Replace("(", @"\(")
                        .Replace(")", @"\)")
                );
                sb.AppendLine();
            }
        }

        for (var index = 0; index < _goods.Length; index++)
        {
            var good = _goods[index];

            if (isSingleGood)
            {
                sb.AppendLine($"[{good.Title}]({good.Url})");
            }
            else
            {
                sb.AppendLine($"{index + 1}. [{good.Title}]({good.Url})");
            }

            sb.AppendLine($"💰 {good.Price}");
        }

        if (!string.IsNullOrWhiteSpace(Settings.StaticFooter))
        {
            sb.AppendLine();
            sb.AppendLine(Settings.StaticFooter);
        }

        return sb.ToString();
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
            ImageLoaded++;
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

            int tgPostId = 0;

            if (PostTelegram)
            {
                Result<int> result = await _telegram.Post(PostText, Attachments.Select(x => x.Image).ToArray());
                tgPostId = result.Value;
            }

            if (PostPinterest)
            {
                if (tgPostId < 1) throw new Exception("Требуется отправить Telegram пост перед Pinterest");

                string tgPostUrl = $"https://t.me/{Settings.TgChannelId[1..]}/{tgPostId}";

                DrawingController drawingController = new();
                List<ImageWithArticle> images = new();
                for (var index = 0; index < _goods.Length; index++)
                {
                    var good = _goods[index];
                    var attach = Attachments.FirstOrDefault(x => x.Id == index + 1);
                    if (attach is null) continue;

                    images.Add(new ImageWithArticle(good.Article, attach.Image));
                }

                Bitmap bmp = drawingController.CreateCollage("WB", images);
                string collagePath = FsManager.SaveBitmapInTempAndGetFullPath(bmp);
                using PinterestDriver driver = await DriverBase.CreateAsync<PinterestDriver>(FsManager, Settings);
                await driver.PostInPinterest(PostText, collagePath);
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

    public void PastImage(object bmpData)
    {
        Bitmap bmp;
        if (bmpData is MemoryStream stream)
        {
            stream.Position = 0;
            bmp = new Bitmap(stream);
        }
        else if (bmpData is byte[] bytes)
        {
            using var memoryStream = new MemoryStream(bytes);
            bmp = new Bitmap(memoryStream);
        }
        else
        {
            throw new NotImplementedException(bmpData.GetType().ToString());
        }
        Attachments.Add(new PreviewImage(Attachments.Count + 1, bmp));
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

    public void RemoveAttach(object e)
    {
        PreviewImage prev = (PreviewImage)e;
        Attachments.Remove(prev);
    }

    public async void CopyAttach(object e)
    {
        PreviewImage prev = (PreviewImage)e;
        Bitmap bmp = prev.Image;
        using var stream = new MemoryStream();
        bmp.Save(stream);
        stream.Position = 0;

        DataObject obj = new();
        obj.Set("image/png", stream.ToArray());
        await WinManager.MainWindow.Clipboard.SetDataObjectAsync(obj);
    }

    public void MorphText(int from, int to, string separator)
    {
        string selectedText = _postText;
        string s1 = selectedText.Insert(from, separator);
        string s2 = s1.Insert(to + 1, separator);
        Console.WriteLine($"before: {selectedText}");
        Console.WriteLine($"insert 1: {s1}");
        Console.WriteLine($"insert 2: {s2}");
        PostText = s2;
    }

    public void MorphText(int from, int to, string lSeparator, string rSeparator)
    {
        string selectedText = _postText;
        string s1 = selectedText.Insert(from, lSeparator);
        string s2 = s1.Insert(to + 1, rSeparator);
        Console.WriteLine($"before: {selectedText}");
        Console.WriteLine($"insert 1: {s1}");
        Console.WriteLine($"insert 2: {s2}");
        PostText = s2;
    }

    public async void SaveAllPreview()
    {
        try
        {
            foreach (var attach in Attachments)
            {
                string? path = await WinManager.SaveFileDialog(WindowManager.SaveFileFormats.PNG);
                if (path is null) continue;
                attach.Image.Save(path);
            }
        }
        catch (Exception ex)
        {
            await WinManager.ShowError(ex);
        }
    }
}
