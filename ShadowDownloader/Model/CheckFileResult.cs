namespace ShadowDownloader.Model;

public class CheckFileResult
{
    public string Id { get; }
    public string Name { get; set; }
    public string Link { get; }
    public string Path { get; }
    public bool CanParallel { get; }
    public long Size { get; }

    public string SizeString => DownloadUtil.ConvertSize(Size);
    public object? Extra { get; }

    public CheckFileResult(string id, string name, string link, string path, bool canParallel, long size,
        object? extra = null)
    {
        Id = id;
        Name = name;
        Link = link;
        Path = path;
        CanParallel = canParallel;
        Size = size;
        Extra = extra;
    }
}