using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
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
            MainWindow mainWindow = new MainWindow();
            mainWindow.Title =
                $"{mainWindow.Title} - {version.Major}.{version.Minor}.{version.Build}";

            MainWindowViewModel vm = new MainWindowViewModel(mainWindow);
            desktop.MainWindow = mainWindow;
            mainWindow.DataContext = vm;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
