namespace ShadowDownloader.Arg;

public class DownloadSpeedArg
{
    /**
     * 下载任务Id
     */
    public int TaskId { get; }

    /**
     * 速度:B/S
     */
    public long Speed { get; }
    
    public DownloadSpeedArg(int taskId, long speed)
    {
        TaskId = taskId;
        Speed = speed;
    }
}