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
    public double Process { get; }

    /**
     * 当前总共已接收
     */
    public long Received { get; }

    public DownloadProcessArg(int taskId, long total, long received)
    {
        TaskId = taskId;
        Total = total;
        Received = received;
        Process = Math.Round(received / (double)total * 100, 2);
    }
}