using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Shavkat_grabber.Models;
using Shavkat_grabber.ViewModels;
using Shavkat_grabber.Views;

namespace Shavkat_grabber;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Version version = Assembly.GetExecutingAssembly().GetName().Version;
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            int argsCount = desktop.Args?.Length ?? 0;
            Console.WriteLine($"args length: {argsCount}");

            AppStartParams appStartParams = new();
            if (argsCount > 0)
            {
                for (int i = 0; i < argsCount; i++)
                {
                    string arg = desktop.Args[i];
                    if (arg == "-a")
                    {
                        appStartParams.Autostart = true;
                        appStartParams.AutostartGoodsCount = int.Parse(desktop.Args[i + 1]);
                        i++;
                    }
                    else if (arg == "-c")
                    {
                        appStartParams.CloseAfterPostings = true;
                    }
                }
            }

            MainWindow mainWindow = new MainWindow();
#if DEBUG
            mainWindow.Title =
                $"{mainWindow.Title} - {version.Major}.{version.Minor}.{version.Build} [DEBUG MODE] ({appStartParams})";
#else
            mainWindow.Title =
                $"{mainWindow.Title} - {version.Major}.{version.Minor}.{version.Build}";
#endif
            MainWindowViewModel vm = new MainWindowViewModel(mainWindow, appStartParams);
            desktop.MainWindow = mainWindow;
            mainWindow.DataContext = vm;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
