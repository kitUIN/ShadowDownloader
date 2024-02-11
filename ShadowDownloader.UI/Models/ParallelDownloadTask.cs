using System.Threading.Tasks;
using ReactiveUI;
using Serilog;

namespace ShadowDownloader.UI.Models;

public class ParallelDownloadTask:DownloadTask
{
    private int _parallelId;
    
    public int ParallelId
    {
        get => _parallelId;
        set => this.RaiseAndSetIfChanged(ref _parallelId, value);
    }
    public ParallelDownloadTask(int taskId,int parallelId, string name, long size) : base(taskId, name, size)
    {
        ParallelId = parallelId;
    }

    private int _dbId;
    public new async Task SaveDbAsync()
    {
        var dbTask = await DbClient.Db.Storageable(new DbParallelDownloadTask
        {
            Id = _dbId,
            TaskId = TaskId,
            ParallelId = ParallelId,
            Percent = Percent,
            Received = Received,
            Size = Size,
            Status = Status,
        }).DefaultAddElseUpdate().ExecuteReturnEntityAsync();
        _dbId = dbTask.Id;
    }
}