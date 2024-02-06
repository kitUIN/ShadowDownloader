using ShadowDownloader.Enum;

namespace ShadowDownloader.Arg;

public class DownloadStatusArg
{
    /**
     * 下载任务Id
     */
    public int TaskId { get; }


    /**
     * 当前状态
     */
    public DownloadStatus Status { get; }

    public DownloadStatusArg(int taskId, DownloadStatus status)
    {
        TaskId = taskId;
        Status = status;
    }
}