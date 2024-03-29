﻿using ShadowDownloader.Enum;
using SqlSugar;

namespace ShadowDownloader.UI.Models;

public class DbParallelDownloadTask
{
    [SugarColumn(IsPrimaryKey = true)] public string Id { get; set; }

    public int TaskId { get; set; }
    public string AdapterId { get; set; } = "";
    public int ParallelId { get; set; }
    public double Percent { get; set; }
    public long Size { get; set; }
    public long Received { get; set; }

    public DownloadStatus Status { get; set; }
}