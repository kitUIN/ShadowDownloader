using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace ShadowDownloader.UI.Converter;

public class PercentConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, 
        CultureInfo culture)
    {
        if (value is double percent && targetType.IsAssignableTo(typeof(string)))
        {
            return percent + "%";
        }
        return new BindingNotification(new InvalidCastException(), 
            BindingErrorType.Error);
    }

    public object ConvertBack(object? value, Type targetType, 
        object? parameter, CultureInfo culture)
    {
        if (value is not string percentString || !targetType.IsAssignableTo(typeof(double)))
            return new BindingNotification(new InvalidCastException(),
                BindingErrorType.Error);
        percentString = percentString[..^2];
        return System.Convert.ToDouble(percentString);
    }
}