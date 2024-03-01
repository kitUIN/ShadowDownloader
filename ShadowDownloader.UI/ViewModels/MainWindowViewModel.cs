using System.Linq;
using System.Threading.Tasks;
using Serilog;
using ShadowDownloader.Model;
using ShadowDownloader.UI.Models;

namespace ShadowDownloader.UI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// 初始化下载任务
    /// </summary>
    /// <param name="taskRecord"></param>
    private void InitTask(DownloadUtil.DownloadTaskRecord taskRecord, string adapterId)
    {
        var task = new DownloadTask(taskRecord.TaskId, taskRecord.Name, taskRecord.Size, adapterId, taskRecord.Parallel,
            taskRecord.TokenSource);
        Tasks.Insert(0, task);
        Log.Information("[Task {TaskId}| Parallel 000]添加下载任务: {Name}, Size:{Size}B", task.TaskId, task.Name, task.Size);
        for (var i = 0; i < taskRecord.ParallelSizeList.Count; i++)
        {
            var pTask = new ParallelDownloadTask(taskRecord.TaskId,
                i + 1, taskRecord.Name, taskRecord.ParallelSizeList[i], adapterId);
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

    private void InitHistory()
    {
        DbClient.Db.Queryable<DbDownloadTask>().ForEach(
            task =>
            {
                var t = new DownloadTask(task);
                DbClient.Db.Queryable<DbParallelDownloadTask>()
                    .Where(x => x.TaskId == t.TaskId)
                    .OrderBy(x => x.ParallelId)
                    .ForEach(pTask => t.Append(new ParallelDownloadTask(pTask)));
                Tasks.Insert(0, t);
            });
    }


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