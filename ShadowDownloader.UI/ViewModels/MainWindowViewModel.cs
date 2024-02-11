using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using Serilog;
using ShadowDownloader.Model;
using ShadowDownloader.UI.Extension;
using ShadowDownloader.UI.Models;

namespace ShadowDownloader.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private async Task<ContentDialogResult> ContentDialogShowAsync(ContentDialog dialog)
    {
        var dialogResult = await dialog.ShowAsync();
        return dialogResult;
    }


    public ReactiveCommand<string, Unit> ShowAddUrlCommand =>
        ReactiveCommand.CreateFromTask<string>(ShowAddUrlAsync);

    public ReactiveCommand<Unit, Unit> DownloadAllCommand =>
        ReactiveCommand.CreateFromTask(DownloadAllFileAsync);

    private async Task DownloadAllFileAsync()
    {
        IsOpenInCheckFile = false;
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
                IsOpenInCheckFile = true;
                IsVisibleInCheckFile = true;
                await CheckFilesInit("cow", res);
            }
        };
        await ContentDialogShowAsync(dialog);
    }

    /// <summary>
    /// 初始化下载任务
    /// </summary>
    /// <param name="taskRecord"></param>
    private void InitTask(DownloadUtil.DownloadTaskRecord taskRecord)
    {
        var task = new DownloadTask(taskRecord.TaskId, taskRecord.Name, taskRecord.Size, taskRecord.Parallel,
            taskRecord.TokenSource);
        Tasks.Insert(0, task);
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