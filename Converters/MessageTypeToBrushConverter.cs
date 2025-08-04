using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.Converters;

public class MessageTypeToBrushConverter : IValueConverter
{
    private readonly IImmutableSolidColorBrush trace = Brushes.LightGray;
    private readonly IImmutableSolidColorBrush success = Brushes.ForestGreen;
    private readonly IImmutableSolidColorBrush warning = Brushes.Orange;
    private readonly IImmutableSolidColorBrush error = Brushes.Red;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return (LogMessageTypes)value switch
        {
            LogMessageTypes.Trace => trace,
            LogMessageTypes.Success => success,
            LogMessageTypes.Warning => warning,
            LogMessageTypes.Error => error,
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
