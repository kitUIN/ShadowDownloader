using Serilog;
using ShadowDownloader.Arg;
using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.ViewModels;

public partial class MainWindowViewModel
{
    public MainWindowViewModel()
    {
        DownloadUtil.DownloadStatusChanged += OnDownloadStatusChanged;
        DownloadUtil.ParallelDownloadStatusChanged += OnParallelDownloadStatusChanged;
        DownloadUtil.DownloadProcessChanged += OnDownloadProcessChanged;
        DownloadUtil.DownloadSpeedChanged += OnDownloadSpeedChanged;
        DownloadUtil.ParallelDownloadProcessChanged += OnParallelDownloadProcessChanged;
    }

    private async void OnParallelDownloadProcessChanged(object? sender, ParallelDownloadProcessArg e)
    {
        if (GetParallelDownloadTask(e.TaskId, e.ParallelId) is { } pTask)
        {
            pTask.Percent = e.Progress;
            pTask.Received = e.Received;
            await pTask.SaveDbAsync();
        }
    }

    private async void OnDownloadSpeedChanged(object? sender, DownloadSpeedArg e)
    {
        if (GetDownloadTask(e.TaskId) is { } task)
        {
            task.Speed = e.Speed;
            await task.SaveDbAsync();
            foreach (var pTask in task.Siblings)
            {
                await pTask.SaveDbAsync();
            }
        }
    }

    private void OnDownloadProcessChanged(object? sender, DownloadProcessArg e)
    {
        if (GetDownloadTask(e.TaskId) is { } task)
        {
            task.Percent = e.Progress;
            task.Received = e.Received;
        }
    }

    private async void OnParallelDownloadStatusChanged(object? sender, ParallelDownloadStatusArg e)
    {
        Log.Information("下载任务[Task {TaskId}| Parallel {ParallelId:000}] 当前状态: {Status}", e.TaskId, e.ParallelId,
            e.Status);
        if (e.Status != DownloadStatus.Pending && GetParallelDownloadTask(e.TaskId, e.ParallelId) is { } pTask)
        {
            pTask.Status = e.Status;
            await pTask.SaveDbAsync();
        }
    }


    private async void OnDownloadStatusChanged(object? sender, DownloadStatusArg e)
    {
        Log.Information("下载任务[Task {TaskId}| Parallel 000] 当前状态: {Status}", e.TaskId, e.Status);
        if (e.Status != DownloadStatus.Pending && GetDownloadTask(e.TaskId) is { } task)
        {
            task.Status = e.Status;
            await task.SaveDbAsync();
            Log.Information("下载任务[Task {TaskId}| Parallel 000] 保存状态: {Status}", e.TaskId, e.Status);
        }
    }
}