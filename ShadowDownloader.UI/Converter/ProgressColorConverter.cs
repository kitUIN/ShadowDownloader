using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.Converter;

public class ProgressColorConverter : IValueConverter
{
    

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not DownloadStatus status)
            return new BindingNotification(new InvalidCastException(),
                BindingErrorType.Error);
        Application.Current!.Styles.TryGetResource("AccentButtonBackground", Application.Current!.ActualThemeVariant,
            out var color);
        return status switch
        {
            DownloadStatus.Error => new SolidColorBrush(Color.FromRgb(248, 49, 47)),
            DownloadStatus.Completed => new SolidColorBrush(Color.FromRgb(0, 210, 106)),
            DownloadStatus.Running => color,
            _ => new SolidColorBrush(Color.FromRgb(219, 155, 52))
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}