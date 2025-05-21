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

        // Проверяем существование файла
        if (!File.Exists(collagePath))
        {
            throw new FileNotFoundException($"Collage file not found: {collagePath}");
        }

        // Создаём контекст с реалистичными настройками
        var contextOptions = new BrowserNewContextOptions
        {
            UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36",
            ViewportSize = new() { Width = 1280, Height = 720 },
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                { "Accept-Language", "en-US,en;q=0.9" },
            },
        };

        if (File.Exists(storageStatePath))
        {
            Console.WriteLine("Storage exists");
            contextOptions.StorageState = await File.ReadAllTextAsync(storageStatePath);
        }

        await using var context = await _browser.NewContextAsync(contextOptions);
        var page = await context.NewPageAsync();

        // Добавляем отладку
        page.PageError += (_, e) => Console.WriteLine($"Page error: {e}");
        page.Console += (_, msg) => Console.WriteLine($"Console: {msg.Text}");
        page.RequestFailed += (_, request) => Console.WriteLine($"Request failed: {request.Url}");

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

        // Проверяем, не перенаправлены ли на страницу логина
        if (page.Url.Contains("login"))
        {
            Console.WriteLine("Redirected to login page, re-authorizing...");
            await Authorize(page);
            await page.GotoAsync("https://ru.pinterest.com/pin-creation-tool/");
        }

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
                await page.ScreenshotAsync(
                    new() { Path = $"error_upload_{DateTime.Now.Ticks}.png" }
                );
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
            await page.WaitForResponseAsync(
                response => response.Url.Contains("upload"),
                new() { Timeout = 15000 }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"File upload failed: {ex.Message}");
            await page.ScreenshotAsync(new() { Path = $"error_upload_{DateTime.Now.Ticks}.png" });
            throw;
        }

        await Task.Delay(3000); // Увеличенная задержка для обработки файла

        // Имитация человеческого поведения
        await page.EvaluateAsync(
            "() => { Object.defineProperty(navigator, 'webdriver', { get: () => undefined }); }"
        );
        await page.Mouse.MoveAsync(200, 200);
        await page.EvaluateAsync("window.scrollTo(0, 200)");

        // Заполняем описание и публикуем
        try
        {
            var descriptionField = await page.WaitForSelectorAsync(
                "div[contenteditable='true']",
                new() { State = WaitForSelectorState.Visible, Timeout = 15000 }
            );
            if (descriptionField != null)
            {
                Console.WriteLine("Found contenteditable field");
                await descriptionField.ClickAsync();
                await descriptionField.FocusAsync();
                await descriptionField.FillAsync(text);
            }
            else
            {
                Console.WriteLine("Contenteditable field not found, trying textarea/input...");
                await page.FillAsync("textarea, input[aria-label*='description']", text);
            }

            await Task.Delay(1000); // Дополнительная задержка перед кликом

            // Ожидаем кнопку "Опубликовать" как ILocator
            var publishButton = page.Locator("button:has-text('Опубликовать')").First;
            await publishButton.WaitForAsync(
                new() { State = WaitForSelectorState.Visible, Timeout = 15000 }
            );
            if (publishButton == null)
            {
                Console.WriteLine("Publish button not found");
                await page.ScreenshotAsync(
                    new() { Path = $"error_publish_button_{DateTime.Now.Ticks}.png" }
                );
                throw new Exception("Publish button not found");
            }

            // Проверяем, активна ли кнопка
            var isEnabled = await publishButton.EvaluateAsync<bool>("el => !el.disabled");
            if (!isEnabled)
            {
                Console.WriteLine(
                    "Publish button is disabled, waiting for it to become enabled..."
                );
                await page.WaitForTimeoutAsync(3000);
                isEnabled = await publishButton.EvaluateAsync<bool>("el => !el.disabled");
                if (!isEnabled)
                {
                    await page.ScreenshotAsync(
                        new() { Path = $"error_publish_button_disabled_{DateTime.Now.Ticks}.png" }
                    );
                    throw new Exception("Publish button is still disabled after waiting");
                }
            }

            await ClickWithRetryAsync(publishButton);

            await page.WaitForResponseAsync(
                response => response.Url.Contains("pin"),
                new() { Timeout = 10000 }
            );
            Console.WriteLine("Post published successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Post failed: {ex.Message}");
            await page.ScreenshotAsync(new() { Path = $"error_post_{DateTime.Now.Ticks}.png" });
            throw;
        }
    }

    private async Task ClickWithRetryAsync(ILocator locator, int maxAttempts = 3)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await locator.ClickAsync(new() { Timeout = 5000 });
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Click attempt {attempt} failed: {ex.Message}");
                if (attempt == maxAttempts)
                    throw;
                await Task.Delay(1000 * attempt);
            }
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
            await page.ScreenshotAsync(new() { Path = $"auth_error_{DateTime.Now.Ticks}.png" });
            throw;
        }
    }
}
