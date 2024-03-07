using System.Threading.Tasks;
using ReactiveUI;
using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.Models;

public class ParallelDownloadTask : DownloadTask
{
    private int _parallelId;

    public int ParallelId
    {
        get => _parallelId;
        set => this.RaiseAndSetIfChanged(ref _parallelId, value);
    }

    public ParallelDownloadTask(int taskId, int parallelId, string name, long size, string adapterId) : base(taskId,
        name, size, adapterId)
    {
        ParallelId = parallelId;
    }

    public ParallelDownloadTask(DbParallelDownloadTask parallelDownloadTask)
    {
        TaskId = parallelDownloadTask.TaskId;
        ParallelId = parallelDownloadTask.ParallelId;
        Size = parallelDownloadTask.Size;
        Percent = parallelDownloadTask.Percent;
        Received = parallelDownloadTask.Received;
        AdapterId = parallelDownloadTask.AdapterId;
        Status = parallelDownloadTask.Status == DownloadStatus.Running
            ? DownloadStatus.Pausing
            : parallelDownloadTask.Status;
    }


    public new async Task SaveDbAsync()
    {
        await DbClient.Db.Storageable(new DbParallelDownloadTask
        {
            Id = $"{TaskId}-{ParallelId}",
            TaskId = TaskId,
            ParallelId = ParallelId,
            AdapterId = AdapterId,
            Percent = Percent,
            Received = Received,
            Size = Size,
            Status = Status,
        }).ExecuteCommandAsync();
    }

    public new async Task RemoveDbAsync()
    {
        await DbClient.Db.Deleteable(new DbParallelDownloadTask
        {
            Id = $"{TaskId}-{ParallelId}"
        }).ExecuteCommandAsync();
    }
}