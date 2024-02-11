using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace ShadowDownloader.UI.Converter;

public class SpeedConverter: IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter,
        CultureInfo culture)
    {
        if (value is not long speed)
            return new BindingNotification(new InvalidCastException(),
                BindingErrorType.Error);
        return DownloadUtil.ConvertSpeed(speed);
    }

    public object ConvertBack(object? value, Type targetType,
        object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}