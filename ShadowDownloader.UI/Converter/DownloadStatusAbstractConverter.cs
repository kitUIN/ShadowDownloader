using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.Converter;

public abstract class DownloadStatusAbstractConverter : IValueConverter
{
    protected abstract object? DownloadStatusConvert(DownloadStatus status, object? parameter);

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DownloadStatus status)
            return new BindingNotification(new InvalidCastException(),
                BindingErrorType.Error);
        return DownloadStatusConvert(status, parameter);
    }


    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}