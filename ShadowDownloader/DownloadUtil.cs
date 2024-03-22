using System.Net;
using System.Net.Http.Headers;
using Serilog;
using ShadowDownloader.Arg;
using ShadowDownloader.Enum;
using ShadowDownloader.Result;

namespace ShadowDownloader;

public static class DownloadUtil
{
    /// <summary>
    /// 线程下载进度更新事件
    /// </summary>
    public static event EventHandler<ParallelDownloadProcessArg>? ParallelDownloadProcessChanged;

    /// <summary>
    /// 线程启动更新事件
    /// </summary>
    public static event EventHandler<ParallelDownloadStatusArg>? ParallelDownloadStatusChanged;

    /// <summary>
    /// 下载启动更新事件
    /// </summary>
    public static event EventHandler<DownloadStatusArg>? DownloadStatusChanged;

    /// <summary>
    /// 下载进度更新事件
    /// </summary>
    public static event EventHandler<DownloadProcessArg>? DownloadProcessChanged;

    /// <summary>
    /// 下载速度更新事件
    /// </summary>
    public static event EventHandler<DownloadSpeedArg>? DownloadSpeedChanged;

    private static HttpClient Client { get; set; } = new();

    public static void SetProxy(Uri uri)
    {
        var webProxy = new WebProxy(uri, BypassOnLocal: false);
        var proxyHttpClientHandler = new HttpClientHandler
        {
            Proxy = webProxy,
            UseProxy = true,
        };
        Client = new HttpClient(proxyHttpClientHandler);
    }

    public static void ClearProxy()
    {
        Client = new HttpClient();
    }


    public static async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req)
    {
        var response = await Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);

        response.EnsureSuccessStatusCode();

        return response;
    }

    private static async Task<HttpResponseMessage> SingleGet(string url, Uri? referer)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (referer != null) request.Headers.Referrer = referer;
        return await SendAsync(request);
    }

    private static async Task<HttpResponseMessage> ParallelGet(string url, RangeHeaderValue range, Uri? referer)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Range = range;
        if (referer != null) request.Headers.Referrer = referer;
        return await SendAsync(request);
    }

    public static async Task WriteParallel(Stream stream, string filePath, long start,
        Action<long> received)
    {
        await using var fs = new FileStream(filePath, FileMode.OpenOrCreate,
            FileAccess.Write, FileShare.Write);
        var buffer = new byte[1024 * 64];
        int bytesRead;
        fs.Seek(start, SeekOrigin.Begin);
        while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length)).ConfigureAwait(false)) > 0)
        {
            fs.Write(buffer, 0, bytesRead);
            received.Invoke(bytesRead);
        }
    }

    public static async Task<CheckParallelResult> CheckParallel(string url, Uri? referer = null)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, url);
        if (referer is not null) request.Headers.Referrer = referer;
        var response = await SendAsync(request);
        var accept = response.Headers.GetValues("Accept-Ranges").FirstOrDefault();
        return new CheckParallelResult(accept == "bytes", response.Content.Headers.ContentLength);
    }

    private static string GetTaskFileNewName(string savePath, string path)
    {
        path = Path.Combine(savePath, path);
        var name = path;
        var i = 0;
        while (File.Exists(name))
        {
            name = Path.Combine(savePath,
                Path.GetFileNameWithoutExtension(path) + $"({++i})" + Path.GetExtension(path));
        }

        return name;
    }

    public static async Task<DownloadTaskRecord> DownloadWithSingle(string link, long length, string name,
        string savePath,
        Configuration config,
        Uri? referer, object? sender)
    {
        var source = new CancellationTokenSource();
        var taskId = ++config.TaskId;
        await config.SaveAsync();
        var path = Path.Combine(savePath, GetTaskFileNewName(savePath, name));
        var filePath = path + $".tmp{taskId}";
        var status = new DownloadStatusArg(taskId, DownloadStatus.Running, name, length);
        var tasks = new List<Task>();
        var downloadNow = 0L; // 当前下载已接收
        var parallelId = 1;
        var parallelNow = 0L;
        var parallelStatus =
            new ParallelDownloadStatusArg(taskId, parallelId, DownloadStatus.Pending, name, length);
        ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);

        tasks.Add(new Task(ParallelDownloadAction, source.Token));
        tasks.Add(new Task(SpeedAction, source.Token));
        return new DownloadTaskRecord(taskId, 1, name, length, path, new List<long> { length }, source, tasks, link,
            savePath, referer is null ? "" : referer.AbsoluteUri);

        async void ParallelDownloadAction()
        {
            try
            {
                parallelStatus.SetStatus(DownloadStatus.Running);
                ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);
                var res = await SingleGet(link, referer);
                await using var stream = await res.Content.ReadAsStreamAsync(source.Token);
                await using var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write,
                    FileShare.Write);
                var buffer = new byte[1024 * 128]; // 每128K汇报一次
                int bytesRead;
                fs.Seek(0, SeekOrigin.Begin);
                while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), source.Token)) > 0)
                {
                    await fs.WriteAsync(buffer.AsMemory(0, bytesRead), source.Token);
                    downloadNow += bytesRead;
                    parallelNow += bytesRead;
                    ParallelDownloadProcessChanged?.Invoke(sender,
                        new ParallelDownloadProcessArg(taskId, parallelId, length, parallelNow));
                    DownloadProcessChanged?.Invoke(sender, new DownloadProcessArg(taskId, length, downloadNow, false));
                }

                parallelStatus.SetStatus(DownloadStatus.Completed);
                ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);
            }
            catch (TaskCanceledException)
            {
                Log.Error("[Task {TaskId}| Parallel {ParallelId:000}] 任务取消", taskId, parallelId);
                parallelStatus.SetStatus(DownloadStatus.Pausing);
                ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Task {TaskId}| Parallel {ParallelId:000}]", taskId, parallelId);
                parallelStatus.SetStatus(DownloadStatus.Error);
                ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);
            }
        }

        async void SpeedAction()
        {
            try
            {
                status.SetStatus(DownloadStatus.Running);
                DownloadStatusChanged?.Invoke(sender, status);
                var last = downloadNow;
                while (last < length)
                {
                    await Task.Delay(1000, source.Token);
                    var speed = (downloadNow - last);
                    DownloadSpeedChanged?.Invoke(sender, new DownloadSpeedArg(taskId, speed));
                    last = downloadNow;
                }

                status.SetStatus(DownloadStatus.Completed);
                DownloadStatusChanged?.Invoke(sender, status);
            }
            catch (TaskCanceledException)
            {
                Log.Error("[Task {TaskId}| Parallel 000] 任务取消", taskId);
                status.SetStatus(DownloadStatus.Pausing);
                DownloadStatusChanged?.Invoke(sender, status);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Task {TaskId}| Parallel 000]", taskId);
                status.SetStatus(DownloadStatus.Error);
                DownloadStatusChanged?.Invoke(sender, status);
            }
        }
    }

    public static async Task<DownloadTaskRecord> DownloadWithParallel(string link, long length, string name,
        string savePath,
        Configuration config,
        Uri? referer, object? sender)
    {
        var parallelSizeList = new List<long>();
        var source = new CancellationTokenSource();
        var taskId = ++config.TaskId;
        await config.SaveAsync();
        var path = Path.Combine(savePath, GetTaskFileNewName(savePath, name));
        var filePath = path + $".tmp{taskId}";
        var status = new DownloadStatusArg(taskId, DownloadStatus.Running, name, length);
        var tasks = new List<Task>();
        var parallel = config.Parallel; // 线程数
        var block = length % parallel == 0 ? length / parallel : length / parallel + 1;
        if (block < config.MinBlockSize) // 如果分割后的大小小于最小块大小,则根据块大小选择线程数
        {
            block = config.MinBlockSize;
            parallel = (int)(length / block);
            if (length % block != 0) parallel++;
        }

        var downloadNow = 0L; // 当前下载已接收
        for (var i = 0; i < parallel; i++)
        {
            var start = i * block;
            var end = start + block;
            if (i == parallel - 1)
            {
                end = length;
            }

            var parallelId = i + 1;
            var parallelSize = end - start;
            parallelSizeList.Add(parallelSize);
            var parallelNow = 0L;
            var parallelStatus =
                new ParallelDownloadStatusArg(taskId, parallelId, DownloadStatus.Pending, name, parallelSize);
            ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);

            tasks.Add(new Task(ParallelDownloadAction, source.Token));
            continue;

            async void ParallelDownloadAction()
            {
                try
                {
                    parallelStatus.SetStatus(DownloadStatus.Running);
                    ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);
                    var res = await ParallelGet(link, new RangeHeaderValue(start, end), referer);
                    await using var stream = await res.Content.ReadAsStreamAsync(source.Token);
                    await using var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write,
                        FileShare.Write);
                    var buffer = new byte[1024 * 128]; // 每128K汇报一次
                    int bytesRead;
                    fs.Seek(start, SeekOrigin.Begin);
                    while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), source.Token)) > 0)
                    {
                        await fs.WriteAsync(buffer.AsMemory(0, bytesRead), source.Token);
                        downloadNow += bytesRead;
                        parallelNow += bytesRead;
                        ParallelDownloadProcessChanged?.Invoke(sender,
                            new ParallelDownloadProcessArg(taskId, parallelId, parallelSize, parallelNow));
                        DownloadProcessChanged?.Invoke(sender,
                            new DownloadProcessArg(taskId, length, downloadNow, false));
                    }

                    parallelStatus.SetStatus(DownloadStatus.Completed);
                    ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);
                }
                catch (TaskCanceledException)
                {
                    Log.Error("[Task {TaskId}| Parallel {ParallelId:000}] 任务取消", taskId, parallelId);
                    parallelStatus.SetStatus(DownloadStatus.Pausing);
                    ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[Task {TaskId}| Parallel {ParallelId:000}]", taskId, parallelId);
                    parallelStatus.SetStatus(DownloadStatus.Error);
                    ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);
                }
            }
        }

        tasks.Add(new Task(SpeedAction, source.Token));
        return new DownloadTaskRecord(taskId, parallel, name, length, path, parallelSizeList, source, tasks, link,
            savePath, referer is null ? "" : referer.AbsoluteUri);

        async void SpeedAction()
        {
            try
            {
                status.SetStatus(DownloadStatus.Running);
                DownloadStatusChanged?.Invoke(sender, status);
                var last = downloadNow;
                while (last < length)
                {
                    await Task.Delay(1000, source.Token);
                    var speed = (downloadNow - last);
                    DownloadSpeedChanged?.Invoke(sender, new DownloadSpeedArg(taskId, speed));
                    last = downloadNow;
                }

                status.SetStatus(DownloadStatus.Completed);
                DownloadStatusChanged?.Invoke(sender, status);
            }
            catch (TaskCanceledException)
            {
                Log.Error("[Task {TaskId}| Parallel 000] 任务取消", taskId);
                status.SetStatus(DownloadStatus.Pausing);
                DownloadStatusChanged?.Invoke(sender, status);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Task {TaskId}| Parallel 000]", taskId);
                status.SetStatus(DownloadStatus.Error);
                DownloadStatusChanged?.Invoke(sender, status);
            }
        }
    }

    public static DownloadTaskRecord RetryDownloadWithParallel(string link, long length, string name,
        string savePath,
        Uri? referer, object? sender, long parallelAvg, int taskId, List<long> parallelCurrentSizeList)
    {
        var parallelSizeList = new List<long>();
        var source = new CancellationTokenSource();
        var path = Path.Combine(savePath, GetTaskFileNewName(savePath, name));
        var filePath = path + $".tmp{taskId}";
        var status = new DownloadStatusArg(taskId, DownloadStatus.Running, name, length);
        var tasks = new List<Task>();
        var parallel = parallelCurrentSizeList.Count; // 线程数
        var downloadNow = parallelCurrentSizeList.Sum(); // 当前下载已接收

        for (var i = 0; i < parallel; i++)
        {
            var start = i * parallelAvg;
            var end = start + parallelAvg;
            if (i == parallel - 1)
            {
                end = length;
            }

            var parallelId = i + 1;
            var parallelSize = end - start;
            parallelSizeList.Add(parallelSize);
            var parallelNow = parallelCurrentSizeList[i];
            if (end == parallelNow) continue;
            var parallelStatus =
                new ParallelDownloadStatusArg(taskId, parallelId, DownloadStatus.Running, name, parallelSize);
            ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);

            tasks.Add(new Task(ParallelDownloadAction, source.Token));
            continue;

            async void ParallelDownloadAction()
            {
                try
                {
                    parallelStatus.SetStatus(DownloadStatus.Running);
                    ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);
                    var res = await ParallelGet(link, new RangeHeaderValue(start + parallelNow, end), referer);
                    await using var stream = await res.Content.ReadAsStreamAsync(source.Token);
                    await using var fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write,
                        FileShare.Write);
                    var buffer = new byte[1024 * 128]; // 每128K汇报一次
                    int bytesRead;
                    fs.Seek(start + parallelNow, SeekOrigin.Begin);
                    while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), source.Token)) > 0)
                    {
                        await fs.WriteAsync(buffer.AsMemory(0, bytesRead), source.Token);
                        downloadNow += bytesRead;
                        parallelNow += bytesRead;
                        ParallelDownloadProcessChanged?.Invoke(sender,
                            new ParallelDownloadProcessArg(taskId, parallelId, parallelSize, parallelNow));
                        DownloadProcessChanged?.Invoke(sender,
                            new DownloadProcessArg(taskId, length, downloadNow));
                    }

                    parallelStatus.SetStatus(DownloadStatus.Completed);
                    ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);
                }
                catch (TaskCanceledException)
                {
                    Log.Error("[Task {TaskId}| Parallel {ParallelId:000}] 任务取消", taskId, parallelId);
                    parallelStatus.SetStatus(DownloadStatus.Pausing);
                    ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "[Task {TaskId}| Parallel {ParallelId:000}]", taskId, parallelId);
                    parallelStatus.SetStatus(DownloadStatus.Error);
                    ParallelDownloadStatusChanged?.Invoke(sender, parallelStatus);
                }
            }
        }

        tasks.Add(new Task(SpeedAction, source.Token));
        return new DownloadTaskRecord(taskId, parallel, name, length, path, parallelSizeList, source, tasks, link,
            savePath, referer is null ? "" : referer.AbsoluteUri);

        async void SpeedAction()
        {
            try
            {
                status.SetStatus(DownloadStatus.Running);
                DownloadStatusChanged?.Invoke(sender, status);
                var last = downloadNow;
                while (last < length)
                {
                    await Task.Delay(1000, source.Token);
                    var speed = (downloadNow - last);
                    DownloadSpeedChanged?.Invoke(sender, new DownloadSpeedArg(taskId, speed));
                    last = downloadNow;
                }

                status.SetStatus(DownloadStatus.Completed);
                DownloadStatusChanged?.Invoke(sender, status);
            }
            catch (TaskCanceledException)
            {
                Log.Error("[Task {TaskId}| Parallel 000] 任务取消", taskId);
                status.SetStatus(DownloadStatus.Pausing);
                DownloadStatusChanged?.Invoke(sender, status);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Task {TaskId}| Parallel 000]", taskId);
                status.SetStatus(DownloadStatus.Error);
                DownloadStatusChanged?.Invoke(sender, status);
            }
        }
    }

    /**
     *  将下载速度转换为易读的格式
     *  @param speed 下载速度（字节/秒）
     */
    public static string ConvertSpeed(long speed)
    {
        var temp = Math.Round(speed / 1024.0, 2);
        if (speed < 1024)
        {
            return $"{speed} B/s";
        }

        if (temp < 1024)
        {
            return $"{temp} K/s";
        }

        temp = Math.Round(temp / 1024.0, 2);
        if (temp < 1024)
        {
            return $"{temp} M/s";
        }

        temp = Math.Round(temp / 1024.0, 2);
        return $"{temp} G/s";
    }

    public static string ConvertSize(long speed)
    {
        var temp = Math.Round(speed / 1024.0, 2);
        if (speed < 1024)
        {
            return $"{speed} B";
        }

        if (temp < 1024)
        {
            return $"{temp} K";
        }

        temp = Math.Round(temp / 1024.0, 2);
        if (temp < 1024)
        {
            return $"{temp} M";
        }

        temp = Math.Round(temp / 1024.0, 2);
        return $"{temp} G";
    }

    public static string ConvertRemainTime(long duration)
    {
        var ts = new TimeSpan(0, 0, Convert.ToInt32(duration));
        var str = ts.Hours switch
        {
            > 0 => $"{ts.Hours:00}:{ts.Minutes:00}:${ts.Seconds:00}",
            0 when ts.Minutes > 0 => $"00:{ts.Minutes:00}:{ts.Seconds:00}",
            _ => ""
        };
        if (ts is { Hours: 0, Minutes: 0 })
        {
            str = $"00:00:{ts.Seconds:00}";
        }

        return str;
    }

    public record DownloadTaskRecord(int TaskId, int Parallel, string Name, long Size, string Path,
        List<long> ParallelSizeList,
        CancellationTokenSource TokenSource, List<Task> ScheduleTasks, string Link, string SavePath, string Referer);
}