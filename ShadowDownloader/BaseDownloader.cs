using System.Diagnostics;
using System.Net.Http.Headers;
using Serilog;
using ShadowDownloader.Arg;
using ShadowDownloader.Enum;

namespace ShadowDownloader;

public class BaseDownloader
{
    /// <summary>
    /// 线程下载进度更新事件
    /// </summary>
    public event EventHandler<ParallelDownloadProcessArg> ParallelDownloadProcessChanged;

    /// <summary>
    /// 线程启动更新事件
    /// </summary>
    public event EventHandler<ParallelDownloadStatusArg> ParallelDownloadStatusChanged;

    /// <summary>
    /// 下载启动更新事件
    /// </summary>
    public event EventHandler<DownloadStatusArg> DownloadStatusChanged;

    /// <summary>
    /// 下载进度更新事件
    /// </summary>
    public event EventHandler<DownloadProcessArg> DownloadProcessChanged;

    /// <summary>
    /// 下载速度更新事件
    /// </summary>
    public event EventHandler<DownloadSpeedArg> DownloadSpeedChanged;

    public Configuration Config { get; }

    public BaseDownloader(Configuration configuration)
    {
        Config = configuration;
    }

    public BaseDownloader()
    {
        Config = Configuration.Default();
    }

    protected async Task<int> DownloadWithParallel(string link, long length, string filePath, int parallel,
        Uri? referer,
        CancellationToken token)
    {
        var taskId = Config.TaskId++;
        await Config.SaveAsync();
        DownloadStatusChanged.Invoke(this, new DownloadStatusArg(taskId, DownloadStatus.Pending));
        var tasks = new List<Task>();
        var block = length % parallel == 0 ? length / parallel : length / parallel + 1;
        if (block < Config.MinBlockSize)
        {
            block = Config.MinBlockSize;
            parallel = (int)(length / block);
            if (length % block != 0) parallel++;
        }
        var total = 0L;
        for (var i = 0; i < parallel; i++)
        {
            var start = i * block;
            var end = start + block;
            if (i == parallel - 1)
            {
                end = length;
            }
            var parallelId = i;
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    ParallelDownloadStatusChanged.Invoke(this,
                        new ParallelDownloadStatusArg(taskId, parallelId, DownloadStatus.Running));
                    var res = await DownloadUtil.ParallelGet(link, new RangeHeaderValue(start, end), referer);
                    await using var steam = await res.Content.ReadAsStreamAsync(token);
                    await DownloadUtil.WriteParallel(steam, filePath, start, received =>
                    {
                        total += received;
                        ParallelDownloadProcessChanged.Invoke(this,
                            new ParallelDownloadProcessArg(taskId, parallelId, end-start, received));
                        DownloadProcessChanged.Invoke(this,
                            new DownloadProcessArg(taskId, length, total));
                    });
                }
                catch (Exception ex)
                {
                    Log.Error("{E}", ex);
                    ParallelDownloadStatusChanged.Invoke(this,
                        new ParallelDownloadStatusArg(taskId, parallelId, DownloadStatus.Error));
                }
            }, token));
        }

        tasks.Add(Task.Run(async () =>
        {
            var last = total;
            while (last < length)
            {
                await Task.Delay(1000, token);
                var speed = (total - last);
                DownloadSpeedChanged.Invoke(this, new DownloadSpeedArg(taskId, speed));
                last = total;
            }

            if (last >= length)
            {
                Trace.WriteLine("完成!");
            }
        }, token));
        await Task.WhenAll(tasks.ToArray());
        DownloadStatusChanged.Invoke(this, new DownloadStatusArg(taskId, DownloadStatus.Running));
        return taskId;
    }
}