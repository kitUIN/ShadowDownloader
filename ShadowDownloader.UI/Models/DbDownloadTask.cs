using SqlSugar;

namespace ShadowDownloader.UI.Models;

public class DbDownloadTask
{
    [SugarColumn(IsPrimaryKey = true)] public int Id { get; set; }
    [SugarColumn(DefaultValue = "")] public string Name { get; set; } = "";
    public int Parallel { get; set; }
    public double Percent { get; set; }
    public long Size { get; set; }
    [SugarColumn(DefaultValue = "")] public string Received { get; set; } = "";
}