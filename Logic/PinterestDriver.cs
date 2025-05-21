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
using Shavkat_grabber.Logic.Abstract;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.Logic;

public class PinterestDriver : DriverBase
{
    public async Task PostInPinterest(string text, string collagePath)
    {
        Console.WriteLine($"Posting to Pinterest: {collagePath}");

        string storageStatePath = "state.json";

        // Создаём контекст с реалистичными настройками
        var contextOptions = new BrowserNewContextOptions
        {
            UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",
            ViewportSize = new() { Width = 1280, Height = 720 },
        };

        if (File.Exists(storageStatePath))
        {
            Console.WriteLine("Storage exists");
            contextOptions.StorageState = await File.ReadAllTextAsync(storageStatePath);
        }

        await using var context = await _browser.NewContextAsync(contextOptions);
        var page = await context.NewPageAsync();

        // Проверяем авторизацию
        await page.GotoAsync("https://ru.pinterest.com/");
        var isLoggedIn = await page.QuerySelectorAsync("[data-test-id='header-profile']") != null;
        if (!isLoggedIn)
        {
            Console.WriteLine("Session expired or not logged in, authorizing...");
            if (File.Exists(storageStatePath))
            {
                File.Delete(storageStatePath); // Удаляем устаревшее состояние
            }
            await Authorize(page);
            await context.StorageStateAsync(new() { Path = storageStatePath });
        }

        Console.WriteLine("Navigating to pin creation tool");
        await page.GotoAsync("https://ru.pinterest.com/pin-creation-tool/");

        // Ожидаем элемент загрузки файла
        try
        {
            await page.WaitForSelectorAsync("#storyboard-upload-input", new() { Timeout = 15000 });
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Upload input not found. Checking if redirected to login...");
            if (page.Url.Contains("login"))
            {
                await Authorize(page);
                await page.GotoAsync("https://ru.pinterest.com/pin-creation-tool/");
                await page.WaitForSelectorAsync(
                    "#storyboard-upload-input",
                    new() { Timeout = 15000 }
                );
            }
            else
            {
                throw;
            }
        }

        // Загружаем файл
        try
        {
            var chooserTask = page.WaitForFileChooserAsync();
            await page.ClickAsync("#storyboard-upload-input");
            var chooser = await chooserTask;
            await chooser.SetFilesAsync(collagePath);
            Console.WriteLine("File uploaded successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"File upload failed: {ex.Message}");
            throw;
        }

        await Task.Delay(1500);

        // Заполняем описание и публикуем
        try
        {
            var div = await page.QuerySelectorAsync("#storyboard-description-field-container");
            Console.WriteLine("div is not null = " + div is not null);
            var subDiv = await div.QuerySelectorAsync("div[role='button']");
            Console.WriteLine("subDiv is not null = " + subDiv is not null);

            await subDiv.FocusAsync();
            await subDiv.PressAsync(text);
            //await page.FillAsync("#storyboard-description-field-container", text);
            await Task.Delay(500);
            await page.GetByText("Опубликовать").ClickAsync();
            await page.WaitForResponseAsync(
                response => response.Url.Contains("pin"),
                new() { Timeout = 10000 }
            );
            Console.WriteLine("Post published successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Post failed: {ex.Message}");
            throw;
        }
    }

    private async Task Authorize(IPage page)
    {
        Console.WriteLine("Starting authorization");

        await page.GotoAsync("https://www.pinterest.com/login/");
        await page.WaitForSelectorAsync("#email", new() { Timeout = 15000 });

        // Заполняем форму авторизации
        await page.FillAsync("#email", _settings.PinterestEmail);
        await Task.Delay(500);
        await page.FillAsync("#password", _settings.PinterestPassword);
        await Task.Delay(500);
        await page.ClickAsync("button[type='submit']");

        // Ждём редиректа или индикатора успешной авторизации
        try
        {
            await page.WaitForURLAsync("https://www.pinterest.com/", new() { Timeout = 15000 });
            Console.WriteLine("Authorization successful");
        }
        catch (TimeoutException)
        {
            Console.WriteLine("Authorization failed: Timeout or redirect issue");
            throw;
        }
    }
}
