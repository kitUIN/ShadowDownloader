using System;
using System.Collections;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ShadowDownloader.UI.Converter;

public class ListNotNullConverter : IValueConverter
{
    public static bool CheckIList(object? value)
    {
        if (value is IList list) return list.Count > 0;
        return false;
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return CheckIList(value);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}