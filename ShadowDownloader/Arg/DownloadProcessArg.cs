namespace ShadowDownloader.Arg;

public class DownloadProcessArg
{
    /**
     * 下载任务Id
     */
    public int TaskId { get; }

    /**
     * 总大小
     */
    public long Total { get; }

    /**
     * 进度:百分比,保留2位小数
     */
    public double Progress { get; }

    /**
     * 当前总共已接收
     */
    public long Received { get; }

    public bool CanParallel { get; }

    public DownloadProcessArg(int taskId, long total, long received, bool canParallel = true)
    {
        TaskId = taskId;
        Total = total;
        Received = received;
        CanParallel = canParallel;
        Progress = Math.Round((received / (double)total * 100), 2);
    }
}