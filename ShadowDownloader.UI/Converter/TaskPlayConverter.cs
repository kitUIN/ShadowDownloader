using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.FluentIcons;
using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.Converter;

public class TaskPlayConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DownloadStatus status)
            return new BindingNotification(new InvalidCastException(),
                BindingErrorType.Error);
        return status switch
        {
            DownloadStatus.Error => FluentIconSymbol.ArrowSync24Regular,
            DownloadStatus.Running => FluentIconSymbol.PauseCircle24Regular,
            _ => FluentIconSymbol.ArrowSync24Regular,
        };
    }


    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}