using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.Converter;

public class TaskRemoveEnableConverter : DownloadStatusAbstractConverter
{
    protected override object? DownloadStatusConvert(DownloadStatus status, object? parameter)
    {
        return status switch
        {
            DownloadStatus.Running => false,
            _ => true,
        };
    }
}