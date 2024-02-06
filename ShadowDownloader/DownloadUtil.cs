using System.Net;
using System.Net.Http.Headers;
using ShadowDownloader.Result;

namespace ShadowDownloader;

public class DownloadUtil
{
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
            return $"{temp} KB/s";
        }

        temp = Math.Round(temp / 1024.0, 2);
        if (temp < 1024)
        {
            return $"{temp} MB/s";
        }

        temp = Math.Round(temp / 1024.0, 2);
        return $"{temp} GB/s";
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
            return $"{temp} KB";
        }

        temp = Math.Round(temp / 1024.0, 2);
        if (temp < 1024)
        {
            return $"{temp} MB";
        }

        temp = Math.Round(temp / 1024.0, 2);
        return $"{temp} GB";
    }
}