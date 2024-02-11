﻿using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using Serilog;
using ShadowDownloader.Arg;
using ShadowDownloader.Enum;
using ShadowDownloader.Result;

namespace ShadowDownloader;

public class DownloadUtil
{
    /// <summary>
    /// 线程下载进度更新事件
    /// </summary>
    public static event EventHandler<ParallelDownloadProcessArg> ParallelDownloadProcessChanged;

    /// <summary>
    /// 线程启动更新事件
    /// </summary>
    public static event EventHandler<ParallelDownloadStatusArg> ParallelDownloadStatusChanged;

    /// <summary>
    /// 下载启动更新事件
    /// </summary>
    public static event EventHandler<DownloadStatusArg> DownloadStatusChanged;

    /// <summary>
    /// 下载进度更新事件
    /// </summary>
    public static event EventHandler<DownloadProcessArg> DownloadProcessChanged;

    /// <summary>
    /// 下载速度更新事件
    /// </summary>
    public static event EventHandler<DownloadSpeedArg> DownloadSpeedChanged;
    public static HttpClient Client { get; set; } = new();

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

    public static async Task<HttpResponseMessage> ParallelGet(string url, RangeHeaderValue range, Uri? referer)
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
    public static async Task<int> DownloadWithParallel(string link, long length,string name, string savePath,
        Configuration config,
        Uri? referer,object sender, CancellationToken token)
    {
        var taskId = config.TaskId++;
        await config.SaveAsync();
        var filePath = Path.Combine(savePath, name + ".tmp");
        var status = new DownloadStatusArg(taskId, DownloadStatus.Pending, name, length);
        DownloadStatusChanged?.Invoke(sender,status);
        var tasks = new List<Task>();
        var parallel = config.Parallel;
        var block = length % parallel == 0 ? length / parallel : length / parallel + 1;
        if (block < config.MinBlockSize)
        {
            block = config.MinBlockSize;
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
            var parallelStatus =
                new ParallelDownloadStatusArg(taskId, parallelId, DownloadStatus.Pending, name, length);
            ParallelDownloadStatusChanged?.Invoke(sender,parallelStatus);
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    parallelStatus.SetStatus(DownloadStatus.Running);
                    ParallelDownloadStatusChanged?.Invoke(sender,parallelStatus);
                    var res = await ParallelGet(link, new RangeHeaderValue(start, end), referer);
                    await using var stream = await res.Content.ReadAsStreamAsync(token);
                    await using var fs = new FileStream(filePath, FileMode.OpenOrCreate,
                        FileAccess.Write, FileShare.Write);
                    var buffer = new byte[1024 * 64];
                    int bytesRead;
                    fs.Seek(start, SeekOrigin.Begin);
                    while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), token)) > 0)
                    {
                        await fs.WriteAsync(buffer.AsMemory(0, bytesRead), token);
                        total += bytesRead;
                        ParallelDownloadProcessChanged?.Invoke(sender,
                            new ParallelDownloadProcessArg(taskId, parallelId, end-start, bytesRead));
                        DownloadProcessChanged?.Invoke(sender,
                            new DownloadProcessArg(taskId, length, total));
                    }
                    parallelStatus.SetStatus(DownloadStatus.Completed);
                    ParallelDownloadStatusChanged?.Invoke(sender,parallelStatus);
                }
                catch (Exception ex)
                {
                    Log.Error("{E}", ex);
                    Trace.WriteLine(ex);
                    parallelStatus.SetStatus(DownloadStatus.Error);
                    ParallelDownloadStatusChanged?.Invoke(sender,parallelStatus);
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
                DownloadSpeedChanged?.Invoke(sender, new DownloadSpeedArg(taskId, speed));
                last = total;
            }

            if (last >= length)
            {
                status.SetStatus(DownloadStatus.Completed);
                DownloadStatusChanged?.Invoke(sender, status);
                Trace.WriteLine("完成!");
            }
            
        }, token));
        await Task.WhenAll(tasks.ToArray());
        status.SetStatus(DownloadStatus.Running);
        DownloadStatusChanged?.Invoke(sender, status);
        return taskId;
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
}