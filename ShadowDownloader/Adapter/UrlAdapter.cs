using ShadowDownloader.Model;

namespace ShadowDownloader.Adapter;

public class UrlAdapter : IAdapter
{
    public static string Id => "url";
    public static string Name => "直链下载";
    public string GetId() => Id;

    public string GetName() => Name;

    public CheckUrlResult CheckUrl(string url)
    {
        return new CheckUrlResult(Id, true, url, url, null, "");
    }

    public async Task<List<CheckFileResult>> CheckFile(CheckUrlResult result, string savePath)
    {
        var link = result.Link;
        var name = Path.GetFileName(result.Link);
        try
        {
            var uri = new Uri(result.Link);
            name = Path.GetFileName(uri.AbsolutePath);
        }
        catch (Exception ignored)
        {
            // ignored
        }

        var resp = await DownloadUtil.CheckParallel(link);
        return new List<CheckFileResult>
            { new(Id, name, link, savePath, resp.CanParallel, resp.Length ?? 0) };
    }

    public async Task<DownloadUtil.DownloadTaskRecord> Download(CheckFileResult result, Configuration config)
    {
        return await DownloadUtil.DownloadWithParallel(result.Link, result.Size,
            result.Name, result.Path, config, null, this);
    }
}