namespace ShadowDownloader.Enum;

public enum DownloadStatus
{
    /**
     * 等待中
     */
    Pending,
    /**
     * 进行中
     */
    Running,
    /**
     * 暂停中
     */
    Pausing,
    /**
     * 报错
     */
    Error,
    /**
     *  已完成
     */
    Completed,
}