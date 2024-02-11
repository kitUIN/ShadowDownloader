using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using ShadowDownloader.Adapter;
using ShadowDownloader.Arg;
using ShadowDownloader.Enum;
using ShadowDownloader.Model;
using ShadowDownloader.Response;
using ShadowDownloader.Result;
using static System.String;

namespace ShadowDownloader;

public class CowAdapter : IAdapter
{
    private const string DownloadDetails = "https://cowtransfer.com/core/api/transfer/share?uniqueUrl={0}";

    private const string DownloadFiles =
        "https://cowtransfer.com/core/api/transfer/share/files?transferGuid={0}&folderId={1}&page={2}&size=20&subContent=false";

    private const string DownloadLinks =
        "https://cowtransfer.com/core/api/transfer/share/download/links?transferGuid={0}&title={1}&fileId={2}";

    private static readonly Regex FileIdPattern = new("[0-9a-f]{14}");

    public static Uri Referer => new("https://cowtransfer.com/");

    public string? GetId(string url)
    {
        var fMatch = FileIdPattern.Match(url);
        return fMatch.Groups.Count == 0 ? null : fMatch.Groups[0].Value;
    }

    public async Task Download(string guid, CowFile cowFile)
    {
        /*if (result is { CanParallel: true, Length: not null })
        {
            var source = new CancellationTokenSource();
            await DownloadWithParallel(link, (long)result.Length, path, Config.Parallel, Referer,
                source.Token);
        }*/
    }

    public async Task GetDownloadDetail(string url)
    {
        var fMatch = FileIdPattern.Match(url);
        if (fMatch.Groups.Count == 0)
        {
            return;
        }

        var fileId = fMatch.Groups[0].Value;
        var (flag, message, guid) = await Fetch(fileId);
        if (flag)
        {
        }
    }

    public async Task<FetchResult> Fetch(string fileId)
    {
        var uri = Format(DownloadDetails, fileId);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await DownloadUtil.SendAsync(request);
        var res = await response.Content.ReadAsStringAsync();
        var resp = JsonConvert.DeserializeObject<FetchResponse>(res);
        if (resp?.Code != "0000" || resp.Data == null) return new FetchResult(false, resp.Message, "");
        return new FetchResult(true, resp.Message, resp.Data.Guid);
    }

    public async Task<FileResponse> FetchItems(string guid, int page = 0, string folderId = "")
    {
        var uri = Format(DownloadFiles, guid, folderId, page);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await DownloadUtil.SendAsync(request);
        var res = await response.Content.ReadAsStringAsync();
        var resp = JsonConvert.DeserializeObject<FileResponse>(res);
        return resp;
    }

    public async Task<LinkResponse> FetchLink(string guid, CowFile cowFile)
    {
        var uri = Format(DownloadLinks, guid, HttpUtility.UrlEncode(cowFile.FileInfo.Title), cowFile.Id);
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        var response = await DownloadUtil.SendAsync(request);
        var res = await response.Content.ReadAsStringAsync();
        var resp = JsonConvert.DeserializeObject<LinkResponse>(res);
        return resp;
    }

    public static string Id => "cow";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string GetId()
    {
        return Id;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public CheckUrlResult CheckUrl(string url)
    {
        var fMatch = FileIdPattern.Match(url);
        return fMatch.Groups.Count == 0
            ? new CheckUrlResult(Id, false, url, "", "", "无法识别的奶牛快传链接")
            : new CheckUrlResult(Id, true, url, fMatch.Groups[0].Value);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<List<CheckFileResult>> CheckFile(CheckUrlResult result, string savePath)
    {
        var list = new List<CheckFileResult>();
        var (flag, message, guid) = await Fetch(result.Link);
        if (flag)
        {
            var files = await FetchItems(guid);
            if (files.CheckCode())
            {
                foreach (var f in files.Data.Files)
                {
                    var res = await FetchLink(guid, f);
                    var name = $"{f.FileInfo.Title}.{f.FileInfo.Format}";
                    // var path = $"{f.Name}.tmp";
                    var link = res.Data[0];
                    var resp = await DownloadUtil.CheckParallel(link, Referer);
                    list.Add(new CheckFileResult(Id,name,link,savePath,resp.CanParallel,resp.Length ?? 0,f));
                }
            }
        }
        return list;
    }

    public async Task<int> Download(CheckFileResult result,Configuration config)
    {
        var source = new CancellationTokenSource();
        return await DownloadUtil.DownloadWithParallel(result.Link, result.Size,result.Name ,result.Path, config, CowAdapter.Referer, this,
            source.Token);
    }
}