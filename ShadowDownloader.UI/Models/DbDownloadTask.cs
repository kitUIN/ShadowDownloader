using ShadowDownloader.Enum;
using SqlSugar;

namespace ShadowDownloader.UI.Models;

public class DbDownloadTask
{
    [SugarColumn(IsPrimaryKey = true)] public int TaskId { get; set; }
    [SugarColumn(DefaultValue = "")] public string Name { get; set; } = "";
    public string AdapterId { get; set; } = "";
    public string Path { get; set; } = "";
    public int Parallel { get; set; }
    public double Percent { get; set; }
    public long Size { get; set; }
    public long Received { get; set; }

    public DownloadStatus Status { get; set; }
}