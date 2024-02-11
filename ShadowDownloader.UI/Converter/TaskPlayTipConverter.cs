using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.Converter;

public class TaskPlayTipConverter : DownloadStatusAbstractConverter
{
    protected override object? DownloadStatusConvert(DownloadStatus status, object? parameter)
    {
        return status switch
        {
            DownloadStatus.Running => "暂停",
            DownloadStatus.Pausing => "开始",
            DownloadStatus.Pending => "开始",
            DownloadStatus.Completed => "",
            DownloadStatus.Error => "重试",
            _ => "开始"
        };
    }
}