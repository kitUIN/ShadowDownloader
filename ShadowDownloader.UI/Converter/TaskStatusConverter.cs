using FluentAvalonia.FluentIcons;
using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.Converter;

public class TaskStatusConverter : DownloadStatusAbstractConverter
{
    protected override object? DownloadStatusConvert(DownloadStatus status, object? parameter)
    {
        return status switch
        {
            DownloadStatus.Error => FluentIconSymbol.DismissCircle24Filled,
            DownloadStatus.Completed => FluentIconSymbol.CheckmarkCircle24Filled,
            DownloadStatus.Running => FluentIconSymbol.Info24Filled,
            _ => FluentIconSymbol.ErrorCircle24Filled
        };
    }
}