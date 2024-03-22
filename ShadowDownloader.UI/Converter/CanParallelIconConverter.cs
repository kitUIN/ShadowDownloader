using System;
using System.Globalization;
using Avalonia.Data.Converters;
using FluentAvalonia.FluentIcons;

namespace ShadowDownloader.UI.Converter;

public class CanParallelIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        return value is true ? FluentIconSymbol.FlashCheckmark24Filled : FluentIconSymbol.FlashFlow24Filled;
    }

    public object ConvertBack(object? value, Type targetType,
        object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}