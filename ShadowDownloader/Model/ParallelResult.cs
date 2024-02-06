using SqlSugar;

namespace ShadowDownloader.Model;

public class ParallelResult
{
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }
    public int TaskId { get; set; }
    public long ParallelId { get; set; }
    public bool Finished { get; set; }
}