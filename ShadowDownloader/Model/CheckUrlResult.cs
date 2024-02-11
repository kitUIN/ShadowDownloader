namespace ShadowDownloader.Model;

public class CheckUrlResult
{
    public string Id { get; }
    public bool Success { get; }
    public string ErrorMessage { get; }
    public string OriginUrl { get; }
    public string Link { get; }
    public object? Extra { get; }

    public CheckUrlResult(string id, bool success, string originUrl, string link, object? extra = null,
        string errorMessage = "")
    {
        Id = id;
        Success = success;
        OriginUrl = originUrl;
        Link = link;
        Extra = extra;
        ErrorMessage = errorMessage;
    }
}