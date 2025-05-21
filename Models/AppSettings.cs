using System;
using System.IO;

namespace Shavkat_grabber.Models;

/// <summary>
/// Настройки программы. Все свойства сериализуются
/// </summary>
public class AppSettings
{
    // Playwright
    public string ChromePath { get; set; }

    // Giga chat
    public string? GigaChatScope { get; set; }
    public string? GigaChatAuthKey { get; set; }
    public string GigaChatPrompt { get; set; }

    // Telegram
    public string? TgChannelId { get; set; }
    public string? TgBotToken { get; set; }

    // Template
    public string? StaticHeader { get; set; }
    public string? StaticFooter { get; set; }

    // Remove bg
    public string? RemoveBgCliPath { get; set; }
    public string? RemoveBgApiKey { get; set; }
    public string? RemoveBgColor { get; set; }

    // Pinterest
    public string? PinterestEmail { get; set; }
    public string? PinterestPassword { get; set; }

    public static AppSettings CreateDefault()
    {
        string sysPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        return new AppSettings
        {
            ChromePath = Path.Combine(sysPath, @"Google\Chrome\Application\chrome.exe"),
            GigaChatPrompt = "Составь мне короткое описание для поста из следующих товаров:",
            RemoveBgColor = "ffffff",
        };
    }
}
