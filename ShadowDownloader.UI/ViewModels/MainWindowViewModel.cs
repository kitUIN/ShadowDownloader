using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using Serilog;
using ShadowDownloader.Arg;
using ShadowDownloader.Enum;
using ShadowDownloader.Model;
using ShadowDownloader.UI.Models;
using ShadowDownloader.UI.Views;

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

    private void OnDownloadSpeedChanged(object? sender, DownloadSpeedArg e)
    {
        if (GetDownloadTask(e.TaskId) is { } task)
        {
            task.Speed = e.Speed;
            var tasks = new List<Task>
            {
                Task.Run(async () => await task.SaveDbAsync())
            };
            tasks.AddRange(task.Siblings.Select(pTask => Task.Run(async () => await pTask.SaveDbAsync())));
            Task.WhenAll(tasks);
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
        if (e.Status == DownloadStatus.Pending)
        {
            if (GetDownloadTask(e.TaskId) is { } task)
            {
                var pTask = new ParallelDownloadTask(e.TaskId, e.ParallelId, e.Name, e.Size);
                task.Append(pTask);
                await task.SaveDbAsync();
                pTask.Status = DownloadStatus.Running;
                await pTask.SaveDbAsync();
                Log.Information("添加下载任务线程: [Task {TaskId}] 线程{ParallelId} , Size:{Size}B", e.TaskId, e.ParallelId,
                    e.Size);
            }
        }
        else if (e.Status != DownloadStatus.Running)
        {
            if (GetParallelDownloadTask(e.TaskId, e.ParallelId) is { } pTask)
            {
                pTask.Status = e.Status;
                await pTask.SaveDbAsync();
            }
        }
    }


    private async void OnDownloadStatusChanged(object? sender, DownloadStatusArg e)
    {
        if (e.Status == DownloadStatus.Pending)
        {
            var task = new DownloadTask(e.TaskId, e.Name, e.Size);
            Tasks.Add(task);
            task.Status = DownloadStatus.Running;
            await task.SaveDbAsync();
            Log.Information("添加下载任务: [Task {TaskId}]{Name}, Size:{Size}B", e.TaskId, e.Name, e.Size);
        }
        else if (e.Status != DownloadStatus.Running)
        {
            if (GetDownloadTask(e.TaskId) is { } task)
            {
                await task.SaveDbAsync();
            }
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

    public async Task<ContentDialogResult> ContentDialogShowAsync(ContentDialog dialog)
    {
        var dialogResult = await dialog.ShowAsync();
        return dialogResult;
    }

    private double _progressValue = 0;

    public double ProgressValue
    {
        get => _progressValue;
        set => this.RaiseAndSetIfChanged(ref _progressValue, value);
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

    private string _speedValue = "0 MB/s";

    public string SpeedValue
    {
        get => _speedValue;
        set => this.RaiseAndSetIfChanged(ref _speedValue, value);
    }

    public ReactiveCommand<string, Unit> ShowAddUrlCommand =>
        ReactiveCommand.CreateFromTask<string>(ShowAddUrlAsync);

    public ReactiveCommand<Unit, Unit> DownloadAllCommand =>
        ReactiveCommand.CreateFromTask(DownloadAllFileAsync);

    public async Task DownloadAllFileAsync()
    {
        IsOpen = false;
        foreach (var checkFile in CheckFiles)
        {
            var taskId = await App.Downloader.Download("cow", checkFile);
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
            if (sender.Content is not TextBox { Text: string url }) return;
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
}