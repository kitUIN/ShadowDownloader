using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.Converter;

public class TaskStatusStringConverter : DownloadStatusAbstractConverter
{
    protected override object? DownloadStatusConvert(DownloadStatus status, object? parameter)
    {
        return status switch
        {
            DownloadStatus.Running => "下载中...",
            DownloadStatus.Pausing => "已暂停...",
            DownloadStatus.Pending => "等待中...",
            DownloadStatus.Completed => "已完成...",
            DownloadStatus.Error => "发生错误...",
            _ => "等待中..."
        };
    }
}