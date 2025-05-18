using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Shavkat_grabber.Views;

public partial class ErrorWindow : Window
{
    public ErrorWindow()
    {
        InitializeComponent();
        DataContext = "Hello World";
    }

    public ErrorWindow(string errorText)
    {
        InitializeComponent();
        DataContext = errorText;
        Console.WriteLine(errorText);

        btnClose.Click += (s, e) => Close();
        btnCopy.Click += BtnCopyOnClick;
    }

    private async void BtnCopyOnClick(object? sender, RoutedEventArgs e)
    {
        await this.Clipboard.SetTextAsync(DataContext.ToString());
    }
}
