using System;
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
        DownloadUtil.DownloadStatusChanged += DownloadUtilOnDownloadStatusChanged;
        DownloadUtil.ParallelDownloadStatusChanged += DownloadUtilOnParallelDownloadStatusChanged;
        DownloadUtil.DownloadProcessChanged += DownloadUtilOnDownloadProcessChanged;
        DownloadUtil.DownloadSpeedChanged += DownloadUtilOnDownloadSpeedChanged;
        DownloadUtil.ParallelDownloadProcessChanged += DownloadUtilOnParallelDownloadProcessChanged;
    }

    private void DownloadUtilOnParallelDownloadProcessChanged(object? sender, ParallelDownloadProcessArg e)
    {
        if (GetDownloadTask(e.TaskId) is { } task)
        {
            if (task.Siblings.FirstOrDefault(x => x.Id == e.ParallelId) is { } pTask)
            {
                pTask.Percent = e.Progress;
            }
        }
    }

    private void DownloadUtilOnDownloadSpeedChanged(object? sender, DownloadSpeedArg e)
    {
        if (GetDownloadTask(e.TaskId) is { } task)
        {
            task.Speed = e.SpeedShow;
        }
    }

    private void DownloadUtilOnDownloadProcessChanged(object? sender, DownloadProcessArg e)
    {
        if (GetDownloadTask(e.TaskId) is { } task)
        {
            task.Percent = e.Progress;
            task.Received = DownloadUtil.ConvertSize(e.Received);
        }
    }

    private void DownloadUtilOnParallelDownloadStatusChanged(object? sender, ParallelDownloadStatusArg e)
    {
        if (e.Status == DownloadStatus.Pending)
        {
            if (GetDownloadTask(e.TaskId) is { } task)
            {
                task.Append(new DownloadTask(e.ParallelId,e.Name,e.Size));
                
            }
        }
    }


    private async void DownloadUtilOnDownloadStatusChanged(object? sender, DownloadStatusArg e)
    {
        if (e.Status == DownloadStatus.Pending)
        {
            var task = new DownloadTask(e.TaskId, e.Name, e.Size);
            Tasks.Add(task);
            await task.SaveDbAsync();
        }
        else if(e.Status == DownloadStatus.Completed)
        {
            if (GetDownloadTask(e.TaskId) is { } task)
            {
                await task.SaveDbAsync();
            }
        }
    }
    private DownloadTask? GetDownloadTask(int taskId)
    {
        return Tasks.FirstOrDefault(x => x.Id == taskId);
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
        foreach (var fcr in await App.Downloader.CheckFile(id,result))
        {
            IsVisibleInCheckFile = false;
            CheckFiles.Add(fcr);
        }
    }
}