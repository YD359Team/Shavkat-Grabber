using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.Converters;

public class ChatMessageTypeToHorizontalAlignmentConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (ChatMessageTypes)value switch
        {
            ChatMessageTypes.Quest => HorizontalAlignment.Right,
            ChatMessageTypes.Answer => HorizontalAlignment.Left,
            ChatMessageTypes.Error => HorizontalAlignment.Center,
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
