namespace ShadowDownloader.Arg;

public class ParallelDownloadProcessArg : DownloadProcessArg
{
    /**
     * 分块Id
     */
    public int ParallelId { get; }

    public ParallelDownloadProcessArg(int taskId, int parallelId, long total, long received) : base(taskId, total,
        received)
    {
        ParallelId = parallelId;
    }
}