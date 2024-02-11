using ShadowDownloader.Enum;

namespace ShadowDownloader.Arg;

public class DownloadStatusArg
{
    /**
     * 下载任务Id
     */
    public int TaskId { get; }
    public string Name { get; }
    public long Size { get; }    

    /**
     * 当前状态
     */
    public DownloadStatus Status { get; private set; }

    public DownloadStatusArg(int taskId, DownloadStatus status, string name, long size)
    {
        TaskId = taskId;
        Status = status;
        Name = name;
        Size = size;
    }

    public void SetStatus(DownloadStatus status)
    {
        Status = status;
    }
}