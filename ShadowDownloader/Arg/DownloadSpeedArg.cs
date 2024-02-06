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

    /**
     * 自动转换后的速度
     */
    public string SpeedShow => DownloadUtil.ConvertSpeed(Speed);

    public DownloadSpeedArg(int taskId, long speed)
    {
        TaskId = taskId;
        Speed = speed;
    }
}