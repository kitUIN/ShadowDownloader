using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using ShadowDownloader.Model;
using ShadowDownloader.UI.Models;

namespace ShadowDownloader.UI.ViewModels;

public partial class MainWindowViewModel
{
    #region 检查文件窗口是否打开

    private bool _isOpenInCheckFile;

    /// <summary>
    /// 检查文件窗口是否打开
    /// </summary>
    public bool IsOpenInCheckFile
    {
        get => _isOpenInCheckFile;
        set => this.RaiseAndSetIfChanged(ref _isOpenInCheckFile, value);
    }

    #endregion

    #region 检查需要下载的文件列表

    private ObservableCollection<CheckFileResult> _checkFiles = new();

    /// <summary>
    /// 检查需要下载的文件列表
    /// </summary>
    public ObservableCollection<CheckFileResult> CheckFiles
    {
        get => _checkFiles;
        set => this.RaiseAndSetIfChanged(ref _checkFiles, value);
    }

    #endregion

    #region 文件下载任务列表

    private ObservableCollection<DownloadTask> _tasks = new();

    /// <summary>
    /// 文件下载任务列表
    /// </summary>
    public ObservableCollection<DownloadTask> Tasks
    {
        get => _tasks;
        set => this.RaiseAndSetIfChanged(ref _tasks, value);
    }

    #endregion

    #region 检查文件窗口中是否显示加载

    private bool _isVisibleInCheckFile;

    /// <summary>
    /// 检查文件窗口中是否显示加载
    /// </summary>
    public bool IsVisibleInCheckFile
    {
        get => _isVisibleInCheckFile;
        set => this.RaiseAndSetIfChanged(ref _isVisibleInCheckFile, value);
    }

    #endregion


    /// <summary>
    /// 从下载列表中获取
    /// </summary>
    /// <param name="taskId">taskId</param>
    private DownloadTask? GetDownloadTask(int taskId)
    {
        return Tasks.FirstOrDefault(x => x.TaskId == taskId);
    }

    /// <summary>
    /// 从下载列表中获取线程
    /// </summary>
    /// <param name="taskId">taskId</param>
    /// <param name="parallelId">parallelId</param>
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