﻿using ShadowDownloader.Enum;

namespace ShadowDownloader.UI.Converter;

public class TaskPlayEnableConverter : DownloadStatusAbstractConverter
{
    protected override object? DownloadStatusConvert(DownloadStatus status, object? parameter)
    {
        return status switch
        {
            DownloadStatus.Completed => false,
            _ => true,
        };
    }
}