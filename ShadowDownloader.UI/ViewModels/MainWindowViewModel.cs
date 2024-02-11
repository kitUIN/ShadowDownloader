using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using Serilog;
using ShadowDownloader.Arg;
using ShadowDownloader.Enum;
using ShadowDownloader.Model;
using ShadowDownloader.UI.Extension;
using ShadowDownloader.UI.Models;

namespace ShadowDownloader.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
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
            var tasks = new List<Task>
            {
                Task.Run(async () => await task.SaveDbAsync())
            };
            tasks.AddRange(task.Siblings.Select(pTask => Task.Run(async () => await pTask.SaveDbAsync())));
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


    private async Task<ContentDialogResult> ContentDialogShowAsync(ContentDialog dialog)
    {
        var dialogResult = await dialog.ShowAsync();
        return dialogResult;
    }

    #region 检查文件窗口是否打开

    private bool _isOpen;

    public bool IsOpen
    {
        get => _isOpen;
        set => this.RaiseAndSetIfChanged(ref _isOpen, value);
    }

    #endregion

    #region 检查需要下载的文件

    private ObservableCollection<CheckFileResult> _checkFiles = new();

    public ObservableCollection<CheckFileResult> CheckFiles
    {
        get => _checkFiles;
        set => this.RaiseAndSetIfChanged(ref _checkFiles, value);
    }

    #endregion

    #region 下载文件

    private ObservableCollection<DownloadTask> _tasks = new();

    public ObservableCollection<DownloadTask> Tasks
    {
        get => _tasks;
        set => this.RaiseAndSetIfChanged(ref _tasks, value);
    }

    #endregion

    #region 检查文件窗口中是否显示加载

    private bool _isVisibleInCheckFile;

    public bool IsVisibleInCheckFile
    {
        get => _isVisibleInCheckFile;
        set => this.RaiseAndSetIfChanged(ref _isVisibleInCheckFile, value);
    }

    #endregion

    private void InitTask(DownloadUtil.DownloadTaskRecord taskRecord)
    {
        var task = new DownloadTask(taskRecord.TaskId, taskRecord.Name, taskRecord.Size, taskRecord.Parallel,
            taskRecord.TokenSource);
        Tasks.Add(task);
        Log.Information("[Task {TaskId}| Parallel 000]添加下载任务: {Name}, Size:{Size}B", task.TaskId, task.Name, task.Size);
        for (var i = 0; i < taskRecord.ParallelSizeList.Count; i++)
        {
            var pTask = new ParallelDownloadTask(taskRecord.TaskId,
                i + 1, taskRecord.Name, taskRecord.ParallelSizeList[i]);
            task.Siblings.Add(pTask);
            Log.Information("[Task {TaskId}| Parallel {ParallelId:000}]添加下载任务线程: Size:{Size}B", pTask.TaskId,
                pTask.ParallelId,
                pTask.Size);
        }
    }

    public ReactiveCommand<string, Unit> ShowAddUrlCommand =>
        ReactiveCommand.CreateFromTask<string>(ShowAddUrlAsync);

    public ReactiveCommand<Unit, Unit> DownloadAllCommand =>
        ReactiveCommand.CreateFromTask(DownloadAllFileAsync);

    private async Task DownloadAllFileAsync()
    {
        IsOpen = false;
        foreach (var checkFile in CheckFiles)
        {
            var taskRecord = await App.Downloader.Download("cow", checkFile);
            InitTask(taskRecord);
            taskRecord.ScheduleTasks.StartAll();
        }
    }

    private async Task ShowAddUrlAsync(string label)
    {
        var text = new TextBox()
        {
            MaxWidth = 280,
            MaxLines = 6,
            TextWrapping = TextWrapping.Wrap,
            Watermark = "输入你要下载的地址"
        };
        var dialog = new ContentDialog
        {
            Title = label,
            PrimaryButtonText = "确定",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = "取消",
            Content = text,
        };
        dialog.PrimaryButtonClick += async (sender, args) =>
        {
            if (sender.Content is not TextBox { Text: { } url }) return;
            var res = App.Downloader.CheckUrl("cow", url);
            if (res.Success)
            {
                IsOpen = true;
                IsVisibleInCheckFile = true;
                await CheckFilesInit("cow", res);
            }
        };
        await ContentDialogShowAsync(dialog);
    }

    private async Task CheckFilesInit(string id, CheckUrlResult result)
    {
        CheckFiles.Clear();
        foreach (var fcr in await App.Downloader.CheckFile(id, result))
        {
            IsVisibleInCheckFile = false;
            CheckFiles.Add(fcr);
        }
    }


    private DownloadTask? GetDownloadTask(int taskId)
    {
        return Tasks.FirstOrDefault(x => x.TaskId == taskId);
    }

    private ParallelDownloadTask? GetParallelDownloadTask(int taskId, int parallelId)
    {
        if (GetDownloadTask(taskId) is { } task)
        {
            for (var i = 0; i < task.Siblings.Count; i++)
            {
                if (task.Siblings[i].ParallelId == parallelId)
                    return task.Siblings[i];
            }
        }

        return null;
    }
}