using ShadowDownloader.Enum;

namespace ShadowDownloader.Arg;

public class ParallelDownloadStatusArg : DownloadStatusArg
{
    /**
     * 分块Id
     */
    public int ParallelId { get; }


    public ParallelDownloadStatusArg(int taskId, int parallelId, DownloadStatus status, 
        string name, long size) : base(taskId, status,name,size)
    {
        ParallelId = parallelId;
    }
}