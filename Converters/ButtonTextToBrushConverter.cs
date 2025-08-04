using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Shavkat_grabber.Converters;

public class ButtonTextToBrushConverter : IValueConverter
{
    private readonly IImmutableSolidColorBrush common = Brushes.Black;
    private readonly IImmutableSolidColorBrush yes = Brushes.ForestGreen;
    private readonly IImmutableSolidColorBrush no = Brushes.Red;

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value.ToString() switch
        {
            "Да" or "Ок" => yes,
            "Нет" or "Отмена" => no,
            _ => common,
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
