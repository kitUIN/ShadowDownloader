using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using FluentAvalonia.FluentIcons;
using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.Converter;

public class TaskStatusConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DownloadStatus status)
            return new BindingNotification(new InvalidCastException(),
                BindingErrorType.Error);

        return status switch
        {
            DownloadStatus.Error => FluentIconSymbol.DismissCircle24Filled,
            DownloadStatus.Completed => FluentIconSymbol.CheckmarkCircle24Filled,
            DownloadStatus.Running => FluentIconSymbol.Info24Filled,
            _ => FluentIconSymbol.ErrorCircle24Filled
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}