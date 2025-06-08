using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ReactiveUI;

namespace Shavkat_grabber.Views.Controls;

public partial class ImgButton : Button
{
    public static readonly StyledProperty<IImage?> ImgSourceProperty = AvaloniaProperty.Register<
        ImgButton,
        IImage?
    >(nameof(ImgSource));

    public static readonly StyledProperty<double> ImgWidthProperty = AvaloniaProperty.Register<
        ImgButton,
        double
    >(nameof(ImgWidth), defaultValue: 32.0);

    public IImage? ImgSource
    {
        get => GetValue(ImgSourceProperty);
        set => SetValue(ImgSourceProperty, value);
    }

    public double ImgWidth
    {
        get => GetValue(ImgWidthProperty);
        set => SetValue(ImgWidthProperty, value);
    }

    static ImgButton()
    {
        // Убедимся, что свойства влияют на рендеринг
        ImgSourceProperty.Changed.AddClassHandler<ImgButton>((x, _) => x.InvalidateVisual());
        ImgWidthProperty.Changed.AddClassHandler<ImgButton>((x, _) => x.InvalidateVisual());
    }
}
