using System;
using System.IO;
using Newtonsoft.Json;
using ReactiveUI;

namespace Shavkat_grabber.Models;

/// <summary>
/// Настройки программы. Все свойства сериализуются
/// </summary>
[JsonObject(MemberSerialization.OptIn)]
public class AppSettings : ReactiveObject
{
    // Playwright
    [JsonProperty]
    public string ChromePath { get; set; }

    // Giga chat
    [JsonProperty]
    public string? GigaChatScope { get; set; }

    [JsonProperty]
    public string? GigaChatAuthKey { get; set; }

    [JsonProperty]
    public string GigaChatPrompt { get; set; }

    // Telegram
    [JsonProperty]
    public string? TgChannelId { get; set; }

    [JsonProperty]
    public string? TgBotToken { get; set; }

    [JsonProperty]
    public string? StaticHeader { get; set; }

    [JsonProperty]
    public string? StaticFooter { get; set; }

    // Remove bg
    [JsonProperty]
    public string? RemoveBgCliPath { get; set; }

    [JsonProperty]
    public string? RemoveBgApiKey { get; set; }

    [JsonProperty]
    public string? RemoveBgColor { get; set; }

    // Pinterest
    [JsonProperty]
    public string? PinterestEmail { get; set; }

    [JsonProperty]
    public string? PinterestPassword { get; set; }

    public static AppSettings CreateDefault()
    {
        string sysPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        return new()
        {
            ChromePath = Path.Combine(sysPath, @"Google\Chrome\Application\chrome.exe"),
            GigaChatPrompt = "Составь мне короткое описание для поста из следующих товаров:",
            RemoveBgColor = "ffffff",
        };
    }
}
