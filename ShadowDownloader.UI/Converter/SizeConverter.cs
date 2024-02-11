using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace ShadowDownloader.UI.Converter;

public class SizeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        if (value is not long size)
            return new BindingNotification(new InvalidCastException(),
                BindingErrorType.Error);
        if (parameter is not null)
        {
            return "已接收:" + DownloadUtil.ConvertSize(size);
        }
        return DownloadUtil.ConvertSize(size);
    }

    public object ConvertBack(object? value, Type targetType,
        object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}