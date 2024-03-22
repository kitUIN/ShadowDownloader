using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ShadowDownloader.UI.Converter;

public class CanParallelITipConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        return value is true ? "支持多线程下载" : "仅单线程下载";
    }

    public object ConvertBack(object? value, Type targetType,
        object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}