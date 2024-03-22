using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ShadowDownloader.Enum;
using ShadowDownloader.UI.Extension;
using ShadowDownloader.UI.ViewModels;

namespace ShadowDownloader.UI.Models;

public class DownloadTask : ReactiveObject
{
    protected DownloadTask()
    {
    }

    public string Link { get; }
    public string SavePath { get; }
    public string Referer { get; }
    public int TaskId { get; protected set; }

    private string _name = "";

    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public string Path { get; set; }

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

    private bool _canParallel;

    public bool CanParallel
    {
        get => _canParallel;
        set => this.RaiseAndSetIfChanged(ref _canParallel, value);
    }

    private ObservableCollection<ParallelDownloadTask> _siblings = new();

    public ObservableCollection<ParallelDownloadTask> Siblings
    {
        get => _siblings;
        set => this.RaiseAndSetIfChanged(ref _siblings, value);
    }

    public string AdapterId { get; protected set; } = "";

    public DownloadTask(int taskId, string name, long size, string adapterId, string link = "", string savePath = "",
        string referer = "", int parallel = 0,
        CancellationTokenSource? source = null, bool canParallel = true
    )
    {
        TaskId = taskId;
        Name = name;
        Size = size;
        Parallel = parallel;
        CancellationTokenSource = source;
        AdapterId = adapterId;
        CanParallel = canParallel;
        Link = link;
        SavePath = savePath;
        Referer = referer;
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
        CanParallel = dbDownloadTask.CanParallel;
        Referer = dbDownloadTask.Referer;
        SavePath = dbDownloadTask.SavePath;
        Link = dbDownloadTask.Link;
        Status = dbDownloadTask.Status == DownloadStatus.Running ? DownloadStatus.Pausing : dbDownloadTask.Status;
        CancellationTokenSource = new CancellationTokenSource();
    }

    public ReactiveCommand<Unit, Unit> StatusCommand =>
        ReactiveCommand.Create(CheckStatus);

    public ReactiveCommand<Unit, Unit> RemoveCommand =>
        ReactiveCommand.CreateFromTask(ShowDoubleCheckDialogAsync);

    private async Task RemoveDb()
    {
        await RemoveAllDbAsync();
        MainWindowViewModel.TaskRemovedInvoke(this);
    }

    private async Task ShowDoubleCheckDialogAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "确定删除?",
            PrimaryButtonText = "确定",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = "取消",
            Content = $"确定删除{Name}?"
        };
        dialog.PrimaryButtonClick += async (_, _) => await RemoveDb();
        await dialog.ShowAsync();
    }


    public void Retry()
    {
        if (CanParallel)
        {
            var referer = string.IsNullOrEmpty(Referer) ? null : new Uri(Referer);
            var taskRecord = DownloadUtil.RetryDownloadWithParallel(Link, Size, Name, SavePath, referer, null,
                Siblings[0].Size, TaskId, Siblings.Select(sibling => sibling.Received).ToList());
            Path = taskRecord.Path;
            taskRecord.ScheduleTasks.StartAll();
            CancellationTokenSource = taskRecord.TokenSource;
        }
    }

    public void Pause()
    {
        CancellationTokenSource?.Cancel();
    }

    private void CheckStatus()
    {
        switch (Status)
        {
            // 正在运行则取消
            case DownloadStatus.Running:
                Pause();
                Status = DownloadStatus.Pausing;
                break;
            // 失败则重试
            case DownloadStatus.Error:
                break;
            // 暂停则继续
            case DownloadStatus.Pausing:
                Retry();
                break;
            case DownloadStatus.Pending:
                break;
            case DownloadStatus.Completed:
                break;
        }
    }

    private CancellationTokenSource? CancellationTokenSource { get; set; }

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
            AdapterId = AdapterId,
            Size = Size,
            Path = Path,
            Status = Status,
            Link = Link,
            SavePath = SavePath,
            Referer = Referer,
            CanParallel = CanParallel
        }).ExecuteCommandAsync();
    }

    public async Task RemoveDbAsync()
    {
        await DbClient.Db.Deleteable(new DbDownloadTask
        {
            TaskId = TaskId
        }).ExecuteCommandAsync();
    }

    public async Task RemoveAllDbAsync()
    {
        foreach (var parallelDownloadTask in Siblings)
        {
            await parallelDownloadTask.RemoveDbAsync();
        }

        await RemoveDbAsync();
    }
}