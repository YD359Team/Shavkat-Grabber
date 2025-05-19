using Avalonia.Controls;
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
        AppSettings = new()
        {
            ChromePath = currentSettings.ChromePath,
            TgBotToken = currentSettings.TgBotToken,
            TgChannelId = currentSettings.TgChannelId,
            GigaChatScope = currentSettings.GigaChatScope,
            GigaChatAuthKey = currentSettings.GigaChatAuthKey,
            GigaChatPrompt = currentSettings.GigaChatPrompt,
            StaticHeader = currentSettings.StaticHeader,
            StaticFooter = currentSettings.StaticFooter,
            RemoveBgCliPath = currentSettings.RemoveBgCliPath,
            RemoveBgApiKey = currentSettings.RemoveBgApiKey,
            RemoveBgColor = currentSettings.RemoveBgColor,
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
        wnd.Close(AppSettings);
    }

    public void Cancel(object eWnd)
    {
        Window wnd = (Window)eWnd;
        wnd.Close(null);
    }
}
