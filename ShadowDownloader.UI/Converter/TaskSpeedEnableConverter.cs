using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.Converter;

public class TaskSpeedEnableConverter : DownloadStatusAbstractConverter
{
    protected override object? DownloadStatusConvert(DownloadStatus status, object? parameter)
    {
        return status switch
        {
            DownloadStatus.Running => true,
            _ => false
        };
    }
}