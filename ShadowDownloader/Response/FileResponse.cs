using Newtonsoft.Json;

namespace ShadowDownloader.Response;

public class FileData
{
    [JsonProperty("files")] public List<CowFile> Files { get; set; }

    [JsonProperty("page")] public int Page { get; set; }

    [JsonProperty("size")] public int Size { get; set; }

    [JsonProperty("totalPage")] public int TotalPage { get; set; }

    [JsonProperty("totalSize")] public int TotalSize { get; set; }

    [JsonProperty("guid")] public object Guid { get; set; }

    [JsonProperty("folderId")] public object FolderId { get; set; }
}

public class CowFile
{
    [JsonProperty("id")] public string Id { get; set; }

    [JsonProperty("owner")] public string Owner { get; set; }

    [JsonProperty("recycle")] public bool Recycle { get; set; }

    [JsonProperty("need_pro")] public bool NeedPro { get; set; }

    [JsonProperty("storage_class")] public string StorageClass { get; set; }

    [JsonProperty("file_type")] public string FileType { get; set; }

    [JsonProperty("analysis_status")] public int AnalysisStatus { get; set; }

    [JsonProperty("audit_status")] public int AuditStatus { get; set; }

    [JsonProperty("repository_id")] public string RepositoryId { get; set; }

    [JsonProperty("created_at")] public long CreatedAt { get; set; }

    [JsonProperty("created_by")] public string CreatedBy { get; set; }

    [JsonProperty("updated_at")] public long UpdatedAt { get; set; }

    [JsonProperty("folder_id")] public string FolderId { get; set; }

    [JsonProperty("folder_name")] public string FolderName { get; set; }

    [JsonProperty("file_info")] public CowFileInfo FileInfo { get; set; }
}

public class CowFileInfo
{
    [JsonProperty("format")] public string Format { get; set; }

    [JsonProperty("size")] public int Size { get; set; }

    [JsonProperty("title")] public string Title { get; set; }

    [JsonProperty("description")] public string Description { get; set; }

    [JsonProperty("preview")] public CowPreview Preview { get; set; }

    [JsonProperty("colors")] public List<object> Colors { get; set; }

    [JsonProperty("origin_url")] public object OriginUrl { get; set; }

    [JsonProperty("theme_color")] public string ThemeColor { get; set; }

    [JsonProperty("extend_previews")] public List<object> ExtendPreviews { get; set; }

    [JsonProperty("music_info")] public object MusicInfo { get; set; }

    [JsonProperty("video_info")] public object VideoInfo { get; set; }
}

public class CowPreview
{
    [JsonProperty("ext")] public object Ext { get; set; }

    [JsonProperty("height")] public object Height { get; set; }

    [JsonProperty("url")] public object Url { get; set; }

    [JsonProperty("width")] public object Width { get; set; }
}

public class FileResponse:BaseResponse
{
    [JsonProperty("data")] public FileData Data { get; set; }
}