using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.Converter;

public class TaskPlayEnableConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DownloadStatus status)
            return new BindingNotification(new InvalidCastException(),
                BindingErrorType.Error);
        return status switch
        {
            DownloadStatus.Completed => false,
            _ => true,
        };
    }


    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}