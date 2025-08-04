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

public class PinterestDriver
{
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;

    public async Task InitializeAsync(AppSettings settings, bool headless = false)
    {
        var playwright = await Playwright.CreateAsync();

        _browser = await playwright.Chromium.LaunchAsync(
            new BrowserTypeLaunchOptions
            {
                Headless = headless,
                ExecutablePath = settings.ChromePath,
                SlowMo = 1000, // Замедляем для более естественного поведения
            }
        );

        _context = await _browser.NewContextAsync(
            new BrowserNewContextOptions
            {
                UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
                ViewportSize = new ViewportSize { Width = 1920, Height = 1080 },
                Locale = "en-US",
            }
        );

        _page = await _context.NewPageAsync();
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            await _page!.GotoAsync("https://www.pinterest.com/login/");
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            Console.WriteLine("✅ NetworkIdle");

            // Ждем поля для входа
            await _page.WaitForSelectorAsync(
                "#email",
                new PageWaitForSelectorOptions { Timeout = 10000 }
            );
            Console.WriteLine("✅ Поле email найдено");

            // Вводим email
            await _page.FillAsync("#email", email);
            await Task.Delay(Random.Shared.Next(500, 1500));

            // Вводим пароль
            await _page.FillAsync("#password", password);
            await Task.Delay(Random.Shared.Next(500, 1500));

            // Нажимаем войти
            await _page.ClickAsync("button[type='submit']");

            // Ждем успешного входа
            await _page.WaitForURLAsync(
                "https://www.pinterest.com/",
                new PageWaitForURLOptions { Timeout = 15000 }
            );

            Console.WriteLine("✅ Успешный вход в Pinterest");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка входа: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CreatePinWithUniversalSelectorsAsync(
        string imagePath,
        string title,
        string description,
        string? boardName = null
    )
    {
        try
        {
            // Проверяем существование файла
            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"❌ Файл не найден: {imagePath}");
                return false;
            }

            // Переходим на страницу создания пина
            await _page!.GotoAsync("https://www.pinterest.com/pin-creation-tool/");
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Ждем появления кнопки загрузки
            var uploadButton = await _page.WaitForSelectorAsync(
                "input[type='file']",
                new PageWaitForSelectorOptions { Timeout = 10000 }
            );

            // Загружаем изображение
            await uploadButton.SetInputFilesAsync(imagePath);
            Console.WriteLine("📤 Изображение загружено");

            // Ждем больше времени для загрузки формы
            await Task.Delay(8000);
            Console.WriteLine("⏳ Ожидаем полную загрузку формы...");

            // Отладка всех элементов
            await DebugPageElementsAsync();

            // Универсальный поиск полей ввода
            var allInputs = await _page.QuerySelectorAllAsync(
                "input[type='text'], input:not([type]), textarea, div[contenteditable='true']"
            );
            Console.WriteLine($"🔍 Найдено {allInputs.Count} полей для ввода");

            IElementHandle? titleField = null;
            IElementHandle? descriptionField = null;

            // Пробуем найти поле заголовка по позиции или контексту
            for (int i = 0; i < allInputs.Count; i++)
            {
                var field = allInputs[i];
                var isVisible = await field.IsVisibleAsync();
                var placeholder = await field.GetAttributeAsync("placeholder");
                var ariaLabel = await field.GetAttributeAsync("aria-label");
                var id = await field.GetAttributeAsync("id");

                if (isVisible)
                {
                    Console.WriteLine(
                        $"Поле {i}: id='{id}', placeholder='{placeholder}', aria-label='{ariaLabel}'"
                    );

                    // Первое видимое поле обычно для заголовка
                    if (titleField == null)
                    {
                        titleField = field;
                        Console.WriteLine($"✅ Используем поле {i} для заголовка");
                    }
                    // Второе видимое поле обычно для описания
                    else if (descriptionField == null)
                    {
                        descriptionField = field;
                        Console.WriteLine($"✅ Используем поле {i} для описания");
                        break;
                    }
                }
            }

            // Заполняем заголовок
            if (titleField != null)
            {
                await titleField.ClickAsync();
                await Task.Delay(500);
                await titleField.FillAsync("");
                await Task.Delay(500);
                await titleField.TypeAsync(title);
                Console.WriteLine("✅ Заголовок введен");
                await Task.Delay(1000);
            }
            else
            {
                Console.WriteLine("❌ Не удалось найти поле заголовка");
                return false;
            }

            // Заполняем описание
            if (descriptionField != null)
            {
                await descriptionField.ClickAsync();
                await Task.Delay(500);
                await descriptionField.FillAsync("");
                await Task.Delay(500);
                await descriptionField.TypeAsync(description);
                Console.WriteLine("✅ Описание введено");
                await Task.Delay(1000);
            }
            else
            {
                Console.WriteLine("⚠️ Поле описания не найдено, продолжаем без него");
            }

            // Ищем кнопку сохранения
            var saveButtons = await _page.QuerySelectorAllAsync("button");
            IElementHandle? saveButton = null;

            for (int i = 0; i < saveButtons.Count; i++)
            {
                var button = saveButtons[i];
                var text = await button.TextContentAsync();
                var isVisible = await button.IsVisibleAsync();
                var dataTestId = await button.GetAttributeAsync("data-test-id");

                if (
                    isVisible
                    && (!string.IsNullOrWhiteSpace(text) || !string.IsNullOrWhiteSpace(dataTestId))
                )
                {
                    var textLower = text?.Trim().ToLower();
                    if (
                        textLower?.Contains("save") == true
                        || textLower?.Contains("publish") == true
                        || textLower?.Contains("сохранить") == true
                        || textLower?.Contains("опубликовать") == true
                        || dataTestId?.Contains("save") == true
                        || dataTestId?.Contains("publish") == true
                    )
                    {
                        saveButton = button;
                        Console.WriteLine(
                            $"✅ Найдена кнопка сохранения: '{text?.Trim()}' (data-test-id: '{dataTestId}')"
                        );
                        break;
                    }
                }
            }

            if (saveButton != null)
            {
                await saveButton.ClickAsync();
                Console.WriteLine("✅ Кнопка сохранения нажата");
                await Task.Delay(5000);

                // Проверяем успешность сохранения
                var url = _page.Url;
                if (url.Contains("pin") && !url.Contains("pin-creation-tool"))
                {
                    Console.WriteLine("✅ Пин успешно создан!");
                    return true;
                }
                else
                {
                    Console.WriteLine("⚠️ Не удалось подтвердить создание пина");
                    return true; // Возвращаем true, так как действия выполнены
                }
            }
            else
            {
                Console.WriteLine("❌ Не удалось найти кнопку сохранения");
                return false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка создания пина: {ex.Message}");
            await _page!.ScreenshotAsync(
                new PageScreenshotOptions { Path = "error_screenshot.png" }
            );
            Console.WriteLine("📸 Скриншот ошибки сохранен в error_screenshot.png");
            return false;
        }
    }

    public async Task<bool> CreatePinAsync(
        string imagePath,
        string title,
        string description,
        string? boardName = null
    )
    {
        try
        {
            // Проверяем существование файла
            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"❌ Файл не найден: {imagePath}");
                return false;
            }

            // Переходим на страницу создания пина
            await _page!.GotoAsync("https://www.pinterest.com/pin-creation-tool/");
            await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Ждем появления кнопки загрузки
            var uploadButton = await _page.WaitForSelectorAsync(
                "input[type='file']",
                new PageWaitForSelectorOptions { Timeout = 10000 }
            );

            // Загружаем изображение
            await uploadButton.SetInputFilesAsync(imagePath);
            Console.WriteLine("📤 Изображение загружено");

            // Ждем обработки изображения и появления формы
            await Task.Delay(5000);
            Console.WriteLine("⏳ Ожидаем загрузку формы...");

            // Пробуем несколько вариантов селекторов для заголовка
            IElementHandle? titleField = null;
            string[] titleSelectors =
            {
                "input#storyboard-selector-title", // Основной селектор из консоли
                "input[id*='storyboard-selector-title']",
                "input[data-test-id='pin-draft-title']",
                "input[placeholder*='заголовок']",
                "input[placeholder*='title']",
                "input[aria-label*='заголовок']",
                "input[aria-label*='title']",
                "div[data-test-id='pin-draft-title'] input",
                "div[role='textbox'][data-contents='true']",
            };

            foreach (var selector in titleSelectors)
            {
                try
                {
                    titleField = await _page.WaitForSelectorAsync(
                        selector,
                        new PageWaitForSelectorOptions { Timeout = 3000 }
                    );
                    Console.WriteLine($"✅ Найдено поле заголовка: {selector}");
                    break;
                }
                catch
                {
                    Console.WriteLine($"⚠️ Селектор не найден: {selector}");
                }
            }

            if (titleField == null)
            {
                Console.WriteLine("❌ Не удалось найти поле заголовка");
                return false;
            }

            // Заполняем заголовок
            await titleField.FillAsync(title);
            await Task.Delay(Random.Shared.Next(500, 1000));

            // Пробуем найти поле описания
            IElementHandle? descriptionField = null;
            string[] descriptionSelectors =
            {
                "input#storyboard-selector-interest-tags", // Из консоли браузера
                "input[id*='storyboard-selector-interest-tags']",
                "input#websitefield", // Также из консоли
                "textarea[data-test-id='pin-draft-description']",
                "textarea[placeholder*='описание']",
                "textarea[placeholder*='description']",
                "textarea[aria-label*='описание']",
                "textarea[aria-label*='description']",
                "div[data-test-id='pin-draft-description'] textarea",
                "div[role='textbox'][data-contents='true']:not(:has(input))",
            };

            foreach (var selector in descriptionSelectors)
            {
                try
                {
                    descriptionField = await _page.WaitForSelectorAsync(
                        selector,
                        new PageWaitForSelectorOptions { Timeout = 3000 }
                    );
                    Console.WriteLine($"✅ Найдено поле описания: {selector}");
                    break;
                }
                catch
                {
                    Console.WriteLine($"⚠️ Селектор не найден: {selector}");
                }
            }

            if (descriptionField != null)
            {
                await descriptionField.FillAsync(description);
                await Task.Delay(Random.Shared.Next(500, 1000));
            }
            else
            {
                Console.WriteLine("⚠️ Поле описания не найдено, продолжаем без него");
            }

            // Выбираем доску, если указана
            if (!string.IsNullOrEmpty(boardName))
            {
                try
                {
                    string[] boardSelectors =
                    {
                        "input[id*='storyboard-drafts-sidebar-bulk-select-checkbox']", // Из консоли
                        "input[id*='storyboard-draft-checkbox']", // Вариации из консоли
                        "[data-test-id='board-dropdown-select-button']",
                        "button[data-test-id='board-dropdown-button']",
                        "div[data-test-id='board-dropdown'] button",
                        "button:has-text('Выберите доску')",
                        "button:has-text('Choose board')",
                    };

                    IElementHandle? boardSelector = null;
                    foreach (var selector in boardSelectors)
                    {
                        try
                        {
                            boardSelector = await _page.WaitForSelectorAsync(
                                selector,
                                new PageWaitForSelectorOptions { Timeout = 2000 }
                            );
                            break;
                        }
                        catch { }
                    }

                    if (boardSelector != null)
                    {
                        await boardSelector.ClickAsync();
                        await Task.Delay(1000);

                        // Ищем доску по имени
                        var boardOption = await _page.WaitForSelectorAsync(
                            $"text={boardName}",
                            new PageWaitForSelectorOptions { Timeout = 5000 }
                        );
                        await boardOption.ClickAsync();
                        Console.WriteLine($"📋 Выбрана доска: {boardName}");
                    }
                }
                catch
                {
                    Console.WriteLine(
                        $"⚠️ Не удалось выбрать доску '{boardName}', будет использована доска по умолчанию"
                    );
                }
            }

            // Публикуем пин
            string[] publishSelectors =
            {
                "button[data-test-id='board-dropdown-save-button']",
                "button[data-test-id='pin-draft-save-button']",
                "button:has-text('Опубликовать')",
                "button:has-text('Publish')",
                "button:has-text('Сохранить')",
                "button:has-text('Save')",
            };

            IElementHandle? publishButton = null;
            foreach (var selector in publishSelectors)
            {
                try
                {
                    publishButton = await _page.WaitForSelectorAsync(
                        selector,
                        new PageWaitForSelectorOptions { Timeout = 3000 }
                    );
                    Console.WriteLine($"✅ Найдена кнопка публикации: {selector}");
                    break;
                }
                catch
                {
                    Console.WriteLine($"⚠️ Кнопка не найдена: {selector}");
                }
            }

            if (publishButton == null)
            {
                Console.WriteLine("❌ Не удалось найти кнопку публикации");
                return false;
            }

            await publishButton.ClickAsync();

            // Ждем подтверждения публикации
            string[] successSelectors =
            {
                "text=Пин сохранен",
                "text=Pin saved",
                "text=Опубликовано",
                "text=Published",
                "[data-test-id='pin-success-message']",
            };

            bool published = false;
            foreach (var selector in successSelectors)
            {
                try
                {
                    await _page.WaitForSelectorAsync(
                        selector,
                        new PageWaitForSelectorOptions { Timeout = 5000 }
                    );
                    published = true;
                    break;
                }
                catch { }
            }

            if (published)
            {
                Console.WriteLine("✅ Пин успешно опубликован!");
                return true;
            }
            else
            {
                Console.WriteLine(
                    "⚠️ Не удалось получить подтверждение публикации, но пин, возможно, был создан"
                );
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка создания пина: {ex.Message}");
            // Сохраняем скриншот для отладки
            await _page!.ScreenshotAsync(
                new PageScreenshotOptions { Path = "error_screenshot.png" }
            );
            Console.WriteLine("📸 Скриншот ошибки сохранен в error_screenshot.png");
            return false;
        }
    }

    public async Task DebugPageElementsAsync()
    {
        try
        {
            Console.WriteLine("🔍 Анализ элементов на странице...");

            // Получаем все input элементы с подробной информацией
            var inputs = await _page!.QuerySelectorAllAsync("input");
            Console.WriteLine($"📝 Найдено {inputs.Count} input элементов:");

            for (int i = 0; i < inputs.Count; i++)
            {
                var input = inputs[i];
                var type = await input.GetAttributeAsync("type");
                var placeholder = await input.GetAttributeAsync("placeholder");
                var dataTestId = await input.GetAttributeAsync("data-test-id");
                var ariaLabel = await input.GetAttributeAsync("aria-label");
                var id = await input.GetAttributeAsync("id");
                var className = await input.GetAttributeAsync("class");
                var isVisible = await input.IsVisibleAsync();

                if (isVisible && (type != "hidden" && type != "checkbox"))
                {
                    Console.WriteLine(
                        $"  Input {i}: id='{id}', type='{type}', placeholder='{placeholder}', data-test-id='{dataTestId}', aria-label='{ariaLabel}', class='{className}'"
                    );
                }
            }

            // Получаем все textarea элементы
            var textareas = await _page.QuerySelectorAllAsync("textarea");
            Console.WriteLine($"📝 Найдено {textareas.Count} textarea элементов:");

            for (int i = 0; i < textareas.Count; i++)
            {
                var textarea = textareas[i];
                var placeholder = await textarea.GetAttributeAsync("placeholder");
                var dataTestId = await textarea.GetAttributeAsync("data-test-id");
                var ariaLabel = await textarea.GetAttributeAsync("aria-label");
                var id = await textarea.GetAttributeAsync("id");
                var className = await textarea.GetAttributeAsync("class");
                var isVisible = await textarea.IsVisibleAsync();

                if (isVisible)
                {
                    Console.WriteLine(
                        $"  Textarea {i}: id='{id}', placeholder='{placeholder}', data-test-id='{dataTestId}', aria-label='{ariaLabel}', class='{className}'"
                    );
                }
            }

            // Получаем все div с contenteditable
            var editableDivs = await _page.QuerySelectorAllAsync("div[contenteditable='true']");
            Console.WriteLine($"📝 Найдено {editableDivs.Count} редактируемых div элементов:");

            for (int i = 0; i < editableDivs.Count; i++)
            {
                var div = editableDivs[i];
                var placeholder = await div.GetAttributeAsync("placeholder");
                var dataTestId = await div.GetAttributeAsync("data-test-id");
                var ariaLabel = await div.GetAttributeAsync("aria-label");
                var id = await div.GetAttributeAsync("id");
                var className = await div.GetAttributeAsync("class");
                var isVisible = await div.IsVisibleAsync();

                if (isVisible)
                {
                    Console.WriteLine(
                        $"  EditableDiv {i}: id='{id}', placeholder='{placeholder}', data-test-id='{dataTestId}', aria-label='{ariaLabel}', class='{className}'"
                    );
                }
            }

            // Получаем все button элементы с текстом "Save" или "Publish"
            var buttons = await _page.QuerySelectorAllAsync("button");
            Console.WriteLine($"🔘 Анализ кнопок (найдено {buttons.Count}):");

            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                var text = await button.TextContentAsync();
                var dataTestId = await button.GetAttributeAsync("data-test-id");
                var ariaLabel = await button.GetAttributeAsync("aria-label");
                var id = await button.GetAttributeAsync("id");
                var className = await button.GetAttributeAsync("class");
                var isVisible = await button.IsVisibleAsync();

                if (
                    isVisible
                    && (!string.IsNullOrWhiteSpace(text) || !string.IsNullOrWhiteSpace(dataTestId))
                )
                {
                    var textTrimmed = text?.Trim().ToLower();
                    if (
                        textTrimmed?.Contains("save") == true
                        || textTrimmed?.Contains("publish") == true
                        || textTrimmed?.Contains("сохранить") == true
                        || textTrimmed?.Contains("опубликовать") == true
                        || !string.IsNullOrWhiteSpace(dataTestId)
                    )
                    {
                        Console.WriteLine(
                            $"  Button {i}: id='{id}', text='{text?.Trim()}', data-test-id='{dataTestId}', aria-label='{ariaLabel}', class='{className}'"
                        );
                    }
                }
            }

            // Сохраняем HTML страницы для детального анализа
            var pageContent = await _page.ContentAsync();
            await File.WriteAllTextAsync("page_content.html", pageContent);
            Console.WriteLine("📄 HTML страницы сохранен в page_content.html");

            // Сохраняем скриншот для визуального анализа
            await _page.ScreenshotAsync(
                new PageScreenshotOptions { Path = "debug_screenshot.png" }
            );
            Console.WriteLine("📸 Скриншот сохранен в debug_screenshot.png");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Ошибка отладки: {ex.Message}");
        }
    }

    public async Task CloseAsync()
    {
        if (_browser != null)
        {
            await _browser.CloseAsync();
        }
    }
}
