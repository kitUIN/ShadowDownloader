using Newtonsoft.Json;

namespace ShadowDownloader.Response;

public class LinkResponse : BaseResponse
{
    [JsonProperty("data")] public List<string> Data { get; set; }
}