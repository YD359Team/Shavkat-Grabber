using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Shavkat_grabber.Views;

public partial class HelpWindow : Window
{
    public HelpWindow()
    {
        InitializeComponent();
        BtnClose.Click += (s, e) => Close();
    }
}
