using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.Models;

public class DownloadTask : ReactiveObject
{
    protected DownloadTask()
    {
    }

    public int TaskId { get; protected set; }

    private string _name = "";

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private int _parallel;

    public int Parallel
    {
        get => _parallel;
        set => this.RaiseAndSetIfChanged(ref _parallel, value);
    }

    private double _percent;

    public double Percent
    {
        get => _percent;
        set => this.RaiseAndSetIfChanged(ref _percent, value);
    }

    public long Size { get; protected set; }
    private long _remainTime;

    public long RemainTime
    {
        get => _remainTime;
        set => this.RaiseAndSetIfChanged(ref _remainTime, value);
    }

    private long _received;

    public long Received
    {
        get => _received;
        set => this.RaiseAndSetIfChanged(ref _received, value);
    }

    private DownloadStatus _status = DownloadStatus.Pending;

    public DownloadStatus Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    private long _speed;

    public long Speed
    {
        get => _speed;
        set
        {
            this.RaiseAndSetIfChanged(ref _speed, value);
            if (Speed > 0)
            {
                RemainTime = (Size - Received) / Speed;
            }
        }
    }

    private ObservableCollection<ParallelDownloadTask> _siblings = new();

    public ObservableCollection<ParallelDownloadTask> Siblings
    {
        get => _siblings;
        set => this.RaiseAndSetIfChanged(ref _siblings, value);
    }

    public string AdapterId { get; protected set; } = "";

    public DownloadTask(int taskId, string name, long size, string adapterId, int parallel = 0,
        CancellationTokenSource? source = null
    )
    {
        TaskId = taskId;
        Name = name;
        Size = size;
        Parallel = parallel;
        CancellationTokenSource = source;
        AdapterId = adapterId;
    }

    public DownloadTask(DbDownloadTask dbDownloadTask)
    {
        TaskId = dbDownloadTask.TaskId;
        Name = dbDownloadTask.Name;
        Size = dbDownloadTask.Size;
        Parallel = dbDownloadTask.Parallel;
        Percent = dbDownloadTask.Percent;
        Received = dbDownloadTask.Received;
        AdapterId = dbDownloadTask.AdapterId;
        Status = dbDownloadTask.Status == DownloadStatus.Running ? DownloadStatus.Pausing : dbDownloadTask.Status;
        CancellationTokenSource = new CancellationTokenSource();
    }

    public ReactiveCommand<Unit, Unit> StatusCommand =>
        ReactiveCommand.Create(CheckStatus);

    public ReactiveCommand<Unit, Unit> RemoveCommand =>
        ReactiveCommand.Create(CheckStatus);

    private void CheckStatus()
    {
        switch (Status)
        {
            // 正在运行则取消
            case DownloadStatus.Running:
                CancellationTokenSource?.Cancel();
                Status = DownloadStatus.Pausing;
                break;
            // 失败则重试
            case DownloadStatus.Error:
                break;
            // 暂停则继续
            case DownloadStatus.Pausing:
                break;
            case DownloadStatus.Pending:
                break;
            case DownloadStatus.Completed:
                break;
        }
    }

    private CancellationTokenSource? CancellationTokenSource { get; }

    public void Append(ParallelDownloadTask task)
    {
        Siblings.Add(task);
        Parallel = Siblings.Count > 0 ? Siblings.Count : 1;
    }

    public async Task SaveDbAsync()
    {
        await DbClient.Db.Storageable(new DbDownloadTask
        {
            TaskId = TaskId,
            Name = Name,
            Parallel = Parallel,
            Percent = Percent,
            Received = Received,
            Size = Size,
            Status = Status,
        }).ExecuteCommandAsync();
    }
}