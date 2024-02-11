using FluentAvalonia.FluentIcons;
using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.Converter;

public class TaskPlayConverter : DownloadStatusAbstractConverter
{
    protected override object? DownloadStatusConvert(DownloadStatus status, object? parameter)
    {
        return status switch
        {
            DownloadStatus.Error => FluentIconSymbol.ArrowSync24Regular,
            DownloadStatus.Running => FluentIconSymbol.PauseCircle24Regular,
            _ => FluentIconSymbol.PlayCircle24Regular,
        };
    }
}