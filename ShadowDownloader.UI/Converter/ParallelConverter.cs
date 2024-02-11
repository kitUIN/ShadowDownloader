using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace ShadowDownloader.UI.Converter;

/// <summary>
/// int To "线程{int}"
/// </summary>
public class ParallelConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {

        if (value is not int parallel)
            return new BindingNotification(new InvalidCastException(),
                BindingErrorType.Error);
        if (parameter is string)
        {
            return "线程数:" + parallel;
        }
        return "线程" + parallel;
    }

    public object ConvertBack(object? value, Type targetType,
        object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}