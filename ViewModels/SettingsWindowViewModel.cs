using System.IO;
using Avalonia.Controls;
using ReactiveUI;
using Shavkat_grabber.Helpers;
using Shavkat_grabber.Logic;
using Shavkat_grabber.Models;
using Shavkat_grabber.Views;

namespace Shavkat_grabber.ViewModels;

public class SettingsWindowViewMode : ChildViewModel
{
    public AppSettings AppSettings { get; set; }

    public SettingsWindowViewMode()
        : base(null, null, null) { }

    public SettingsWindowViewMode(
        FileSystemManager fsManager,
        WindowManager manager,
        AppSettings? currentSettings
    )
        : base(fsManager, manager, currentSettings)
    {
        UpdateVisibleSettings(currentSettings);
    }

    private void UpdateVisibleSettings(AppSettings fromSettings)
    {
        AppSettings = new()
        {
            ChromePath = fromSettings.ChromePath,
            TgBotToken = fromSettings.TgBotToken,
            TgChannelId = fromSettings.TgChannelId,
            GigaChatScope = fromSettings.GigaChatScope,
            GigaChatAuthKey = fromSettings.GigaChatAuthKey,
            GigaChatPrompt = fromSettings.GigaChatPrompt,
            StaticHeader = fromSettings.StaticHeader,
            StaticFooter = fromSettings.StaticFooter,
            RemoveBgCliPath = fromSettings.RemoveBgCliPath,
            RemoveBgApiKey = fromSettings.RemoveBgApiKey,
            RemoveBgColor = fromSettings.RemoveBgColor,
            PinterestEmail = fromSettings.PinterestEmail,
            PinterestPassword = fromSettings.PinterestPassword,
        };
    }

    public void Save(object eWnd)
    {
        SettingsWindow wnd = (SettingsWindow)eWnd;
        AppSettings.ChromePath = wnd.tbChromePath.Text;
        AppSettings.GigaChatAuthKey = wnd.tbGigaChatAuthKey.Text;
        AppSettings.GigaChatScope = wnd.tbGigaChatScope.Text;
        AppSettings.GigaChatPrompt = wnd.tbGigaChatPrompt.Text;
        AppSettings.TgBotToken = wnd.tbBotToken.Text;
        AppSettings.TgChannelId = wnd.tbChId.Text;
        AppSettings.StaticHeader = wnd.tbStaticHeader.Text;
        AppSettings.StaticFooter = wnd.tbStaticFooter.Text;
        AppSettings.RemoveBgCliPath = wnd.tbRemoveBgCliPath.Text;
        AppSettings.RemoveBgApiKey = wnd.tbRemoveBgApi.Text;
        AppSettings.RemoveBgColor = wnd.tbRemoveBgColor.Text;
        AppSettings.PinterestEmail = wnd.tbPinterestEmail.Text;
        AppSettings.PinterestPassword = wnd.tbPinterestPassword.Text;
        wnd.Close(AppSettings);
    }

    public void Cancel(object eWnd)
    {
        Window wnd = (Window)eWnd;
        wnd.Close(null);
    }

    public async void LoadFromFile()
    {
        string? path = await WinManager.OpenFileDialog(WindowManager.FileFormats.Json);
        if (path is null)
            return;

        AppSettings settings = SerializeHelper.Deserialize<AppSettings>(File.ReadAllText(path));
        UpdateVisibleSettings(settings);
        this.RaisePropertyChanged(nameof(AppSettings));
    }
}
