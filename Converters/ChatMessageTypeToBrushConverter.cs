using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.Converters;

public class ChatMessageTypeToBrushConverter : IValueConverter
{
    private readonly IImmutableSolidColorBrush user = Brushes.DodgerBlue;
    private readonly IImmutableSolidColorBrush bot = Brushes.LightGray;
    private readonly IImmutableSolidColorBrush error = Brushes.Red;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (ChatMessageTypes)value switch
        {
            ChatMessageTypes.Quest => user,
            ChatMessageTypes.Answer => bot,
            ChatMessageTypes.Error => error,
            _ => throw new NotImplementedException(),
        };
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}
