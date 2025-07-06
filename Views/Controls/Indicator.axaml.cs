using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Telegram.Bot.Types;

namespace Shavkat_grabber.Views.Controls;

public partial class Indicator : UserControl
{
    public static readonly StyledProperty<bool> IsCheckedProperty = AvaloniaProperty.Register<
        Indicator,
        bool
    >(nameof(IsChecked), defaultValue: false);

    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public Indicator()
    {
        InitializeComponent();
    }
}
