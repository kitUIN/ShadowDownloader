using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using ReactiveUI;
using ShadowDownloader.Enum;
using SqlSugar;

namespace ShadowDownloader.UI.Models;

public class DownloadTask: ReactiveObject
{
    private int _taskId;
    
    public int TaskId
    {
        get => _taskId;
        set => this.RaiseAndSetIfChanged(ref _taskId, value);
    }
    
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
    
    public long Size { get; }
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
            RemainTime = (Size - Received) / _speed;
        }
    }
    private ObservableCollection<ParallelDownloadTask> _siblings = new();

    public ObservableCollection<ParallelDownloadTask> Siblings
    {
        get => _siblings;
        set => this.RaiseAndSetIfChanged(ref _siblings, value);
    }

    public DownloadTask(int taskId, string name, long size)
    {
        TaskId = taskId;
        Name = name;
        Size = size;
        Parallel = 0;
    }
    
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