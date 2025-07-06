using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Shavkat_grabber.Models;

namespace Shavkat_grabber.Converters;

public class BoolToBrushConverter : IValueConverter
{
    private readonly IImmutableSolidColorBrush yes = Brushes.Lime;
    private readonly IImmutableSolidColorBrush no = Brushes.Red;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool and true ? yes : no;
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
