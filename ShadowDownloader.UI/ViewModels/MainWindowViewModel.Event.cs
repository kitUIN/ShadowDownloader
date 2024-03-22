using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using ShadowDownloader.Arg;
using ShadowDownloader.Enum;
using ShadowDownloader.UI.Models;

namespace ShadowDownloader.UI.ViewModels;

public partial class MainWindowViewModel
{
    public static event EventHandler<DownloadTask>? TaskRemoved;
    public static event EventHandler<string>? TaskDialogShowed;

    public static void TaskRemovedInvoke(DownloadTask task)
    {
        TaskRemoved?.Invoke(null, task);
    }

    private void TaskDialogShowAsync(string taskDialogName)
    {
        TaskDialogShowed?.Invoke(this, taskDialogName);
    }

    public MainWindowViewModel()
    {
        DownloadUtil.DownloadStatusChanged += OnDownloadStatusChanged;
        DownloadUtil.ParallelDownloadStatusChanged += OnParallelDownloadStatusChanged;
        DownloadUtil.DownloadProcessChanged += OnDownloadProcessChanged;
        DownloadUtil.DownloadSpeedChanged += OnDownloadSpeedChanged;
        DownloadUtil.ParallelDownloadProcessChanged += OnParallelDownloadProcessChanged;
        TaskRemoved += (_, task) => Tasks.Remove(task);
        InitHistory();
        CheckFiles.CollectionChanged += (_, _) => CheckFileCount = CheckFiles.Count > 0;
    }

    private void OnParallelDownloadProcessChanged(object? sender, ParallelDownloadProcessArg e)
    {
        if (GetParallelDownloadTask(e.TaskId, e.ParallelId) is { } pTask && pTask.Status != DownloadStatus.Completed)
        {
            pTask.Percent = e.Progress;
            pTask.Received = e.Received;
        }
    }

    private async void OnDownloadSpeedChanged(object? sender, DownloadSpeedArg e)
    {
        if (GetDownloadTask(e.TaskId) is { } task)
        {
            task.Speed = e.Speed;
            var tasks = task.Siblings
                .Where(x => x.Status != DownloadStatus.Completed)
                .Select(pTask => pTask.SaveDbAsync())
                .ToList();
            tasks.Add(task.SaveDbAsync());
            await Task.WhenAll(tasks);
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
        if (e.Status != DownloadStatus.Pending &&
            GetParallelDownloadTask(e.TaskId, e.ParallelId) is { } pTask)
        {
            pTask.Status = e.Status;
            await pTask.SaveDbAsync();
        }
    }


    private async void OnDownloadStatusChanged(object? sender, DownloadStatusArg e)
    {
        Log.Information("下载任务[Task {TaskId}| Parallel 000] 当前状态: {Status}", e.TaskId, e.Status);
        if (GetDownloadTask(e.TaskId) is { } task)
        {
            switch (e.Status)
            {
                case DownloadStatus.Pending:
                    return;
                case DownloadStatus.Completed:
                    try
                    {
                        var newFile = task.Path;
                        var oldFile = newFile + $".tmp{task.TaskId}";
                        File.Move(oldFile, newFile);
                    }
                    catch (Exception exception)
                    {
                        Log.Error("下载任务[Task {TaskId}| Parallel 000] 错误:{E}", e.TaskId, exception);
                        task.Status = DownloadStatus.Error;
                        await task.SaveDbAsync();
                    }

                    break;
                case DownloadStatus.Running:
                    break;
                case DownloadStatus.Pausing:
                    break;
                case DownloadStatus.Error:
                    break;
            }

            task.Status = e.Status;
            await task.SaveDbAsync();
            Log.Information("下载任务[Task {TaskId}| Parallel 000] 保存状态: {Status}", e.TaskId, e.Status);
        }
    }
}