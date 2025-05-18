using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Media.Imaging;
using Microsoft.Playwright;
using Shavkat_grabber.Extensions;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.Logic;

public delegate void LogMessageDelegate(LogMessage logMessage);

public class Driver
{
    private readonly IPlaywright _playwright;
    private readonly IBrowser _browser;
    private readonly IPage _page;

    public event LogMessageDelegate OnLogMessage;
    public event EventHandler OnScaningEnd;

    private Driver(IPlaywright playwright, IBrowser browser, IPage page)
    {
        _playwright = playwright;
        _browser = browser;
        _page = page;
    }

    public static async Task<Driver> CreateAsync(AppSettings appSettings)
    {
        var playwright = await Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(
            new() { Headless = false, ExecutablePath = appSettings.ChromePath }
        );
        var page = await browser.NewPageAsync();
        return new Driver(playwright, browser, page);
    }

    public async IAsyncEnumerable<Good> StartGrab(string[] keyWords, int count)
    {
        await _page.GotoAsync("https://www.wildberries.ru/");
        await Task.Delay(250);
        await _page.WaitForSelectorAsync(".cookies__btn");
        await Task.Delay(2000);

        var cookiesBtn = await _page.QuerySelectorAsync("button.cookies__btn");
        await cookiesBtn.ClickAsync();
        SendLogMessage(new LogMessage("Cookies button clicked", LogMessageTypes.Success));

        var searchBar = await _page.QuerySelectorAsync("input#searchInput");
        var submitBtn = await _page.QuerySelectorAsync("button#applySearchBtn");
        if (searchBar == null || submitBtn == null)
        {
            SendLogMessage(
                new LogMessage("Search bar or submit button not found", LogMessageTypes.Error)
            );
            yield break;
        }

        foreach (var keyword in keyWords)
        {
            SendLogMessage(new LogMessage($"Processing keyword: {keyword}", LogMessageTypes.Trace));

            await searchBar.FocusAsync();
            await searchBar.FillAsync(keyword);
            await Task.Delay(750);
            await submitBtn.ClickAsync();
            await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await Task.Delay(2000);
            await _page.Keyboard.DownAsync("ArrowDown");

            var cards = await _page.QuerySelectorAllAsync("a.product-card__link");
            var cardUrls = await Task.WhenAll(
                cards.Select(async x => await x.GetAttributeAsync("href"))
            );

            SendLogMessage(new LogMessage($"Link count " + cardUrls.Length, LogMessageTypes.Trace));

            for (int i = 0; i < Math.Min(count, cards.Count); i++)
            {
                var card = cards[i];
                string url = cardUrls[i];
                await card.HoverAsync();
                var goodPage = await _browser.NewPageAsync();

                var good = await TryGrabGood(goodPage, cards, url);

                if (good is not null)
                {
                    yield return good;

                    SendLogMessage(new LogMessage($"finish!", LogMessageTypes.Success));
                }
                else
                {
                    SendLogMessage(new LogMessage($"error!", LogMessageTypes.Error));
                }

                await goodPage.CloseAsync();
            }
        }
        OnScaningEnd?.Invoke(this, EventArgs.Empty);
        await DisposeAsync();
    }

    public async Task PostInPinterest()
    {
        await _page.GotoAsync("https://ru.pinterest.com/");
        await Task.Delay(1000);

        await _page.GotoAsync("https://ru.pinterest.com/pin-creation-tool/");

        Console.WriteLine("wait h1");
        await _page.WaitForSelectorAsync("h1");
    }

    private async Task<Good?> TryGrabGood(
        IPage goodPage,
        IReadOnlyList<IElementHandle> cards,
        string url
    )
    {
        try
        {
            await goodPage.GotoAsync(url);
            await goodPage.WaitForSelectorAsync("button.order__button.btn-main");

            var title =
                await (
                    await goodPage.QuerySelectorAsync("h1.product-page__title")
                )?.TextContentAsync() ?? "N/A";
            SendLogMessage(new LogMessage($"- title", LogMessageTypes.Success));
            var article =
                await (await goodPage.QuerySelectorAsync("#productNmId"))?.TextContentAsync()
                ?? "N/A";
            SendLogMessage(new LogMessage($"- article", LogMessageTypes.Success));
            var price =
                await (
                    await goodPage.QuerySelectorAsync(".price-block__wallet-price")
                )?.TextContentAsync() ?? "N/A";
            SendLogMessage(new LogMessage($"- price", LogMessageTypes.Success));
            var oldPrice =
                await (
                    await goodPage.QuerySelectorAsync(".price-block__old-price")
                )?.TextContentAsync() ?? "N/A";
            SendLogMessage(new LogMessage($"- old price", LogMessageTypes.Success));
            var vendor =
                await (
                    await goodPage.QuerySelectorAsync(".product-page__header-brand")
                )?.TextContentAsync() ?? "N/A";
            SendLogMessage(new LogMessage($"- vendor", LogMessageTypes.Success));
            var imgUrl =
                await (
                    await goodPage.QuerySelectorAsync(".zoom-image-container img")
                )?.GetAttributeAsync("src") ?? "N/A";
            SendLogMessage(new LogMessage($"- image url", LogMessageTypes.Success));
            var rating =
                await (
                    await goodPage.QuerySelectorAsync(".product-review__rating")
                )?.TextContentAsync() ?? "N/A";
            SendLogMessage(new LogMessage($"- rating", LogMessageTypes.Success));

            Good good = new Good
            {
                Article = article,
                Url = url,
                Title = title,
                Price = price,
                OldPrice = oldPrice,
                Vendor = vendor,
                ImageUrl = imgUrl,
                Rating = rating,
            };
            return good;
        }
        catch (Exception e)
        {
            SendLogMessage(new(e.GetMessage(), LogMessageTypes.Error));
            return null;
        }
    }

    private void SendLogMessage(LogMessage logMessage)
    {
        Console.WriteLine(logMessage.Message);
        OnLogMessage?.Invoke(logMessage);
    }

    public async ValueTask DisposeAsync()
    {
        await _page.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    public async Task RemoveBackground(ObservableCollection<PreviewImage> attachments)
    {
        string[] fileNames = SaveBitmaps(attachments.Select(x => x.Image));

        await _page.GotoAsync("https://www.remove.bg/ru/upload");
        await Task.Delay(1750);

        var chooser = _page.WaitForFileChooserAsync();
        await _page.GetByText("Загрузить изображение").ClickAsync();
        await chooser.Result.SetFilesAsync(fileNames.First());

        if (fileNames.Length > 1)
        {
            await Task.Delay(1500);
            foreach (var fileName in fileNames.Skip(1))
            {
                chooser = _page.WaitForFileChooserAsync();
                await _page.ClickAsync("#footer button");
                await chooser.Result.SetFilesAsync(fileName);
                await Task.Delay(1500);
            }
        }

        await Task.Delay(1500 + 750 * fileNames.Length);

        var previews = await _page.QuerySelectorAllAsync("img.checkerboard");
        int index = 0;
        foreach (var prev in previews)
        {
            await prev.ClickAsync();
            await Task.Delay(500);
            var downloader = _page.WaitForDownloadAsync();
            await _page.GetByText("Скачать ").ClickAsync();
            await downloader.Result.SaveAsAsync(fileNames[index]);
            index++;
        }
    }

    private string[] SaveBitmaps(IEnumerable<Bitmap> attachments)
    {
        string path = Path.Combine(Environment.CurrentDirectory, "Temp");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        else
        {
            foreach (string file in Directory.GetFiles(path))
            {
                File.Delete(file);
            }
        }

        string[] arr = new string[attachments.Count()];
        int i = 0;
        foreach (var bmp in attachments)
        {
            string fullName = Path.Combine(path, $"img{i}.png");
            arr[i] = fullName;
            Console.WriteLine($"save file {fullName}");

            bmp.Save(fullName);
            i++;
        }

        return arr;
    }
}
