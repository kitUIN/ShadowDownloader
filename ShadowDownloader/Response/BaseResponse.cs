using Newtonsoft.Json;

namespace ShadowDownloader.Response;

public class BaseResponse
{
    [JsonProperty("code")] public string Code { get; set; }

    [JsonProperty("message")] public string Message { get; set; }

    [JsonProperty("tn")] public string Tn { get; set; }

    public bool CheckCode() => Code == "0000";
}