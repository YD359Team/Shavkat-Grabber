using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Shavkat_grabber.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();

        recColor.Fill = new SolidColorBrush(Colors.White);

        tbRemoveBgColor.TextChanged += OnTbRemoveBgColorOnTextChanged;
    }

    private void OnTbRemoveBgColorOnTextChanged(object? s, TextChangedEventArgs e)
    {
        if (tbRemoveBgColor.Text.Length != 6)
            return;

        try
        {
            recColor.Fill = new SolidColorBrush(
                Color.FromUInt32(
                    uint.Parse("ff" + tbRemoveBgColor.Text, NumberStyles.AllowHexSpecifier)
                )
            );
        }
        catch { }
    }
}
