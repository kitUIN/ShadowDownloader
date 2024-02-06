// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

using Newtonsoft.Json;

namespace ShadowDownloader.Response;

public class FetchResponse :BaseResponse
{
    [JsonProperty("data")] public FetchData? Data { get; set; }
}

public class FetchData
{
    [JsonProperty("payEnabled")] public bool PayEnabled { get; set; }

    [JsonProperty("payStatus")] public bool PayStatus { get; set; }

    [JsonProperty("skuId")] public object SkuId { get; set; }

    [JsonProperty("skuPrice")] public object SkuPrice { get; set; }

    [JsonProperty("guid")] public string Guid { get; set; }

    [JsonProperty("transferName")] public string TransferName { get; set; }

    [JsonProperty("transferMessage")] public string TransferMessage { get; set; }

    [JsonProperty("uniqueUrl")] public string UniqueUrl { get; set; }

    [JsonProperty("needPassword")] public bool NeedPassword { get; set; }

    [JsonProperty("expireAt")] public string ExpireAt { get; set; }

    [JsonProperty("validDays")] public int ValidDays { get; set; }

    [JsonProperty("enableDownload")] public bool EnableDownload { get; set; }

    [JsonProperty("enablePreview")] public bool EnablePreview { get; set; }

    [JsonProperty("enableSaveto")] public bool EnableSaveto { get; set; }

    [JsonProperty("uploadState")] public int UploadState { get; set; }

    [JsonProperty("deleted")] public bool Deleted { get; set; }

    [JsonProperty("tag")] public int Tag { get; set; }

    [JsonProperty("dataTag")] public int DataTag { get; set; }

    [JsonProperty("status")] public int Status { get; set; }

    [JsonProperty("fileAmount")] public int FileAmount { get; set; }

    [JsonProperty("folderAmount")] public int FolderAmount { get; set; }

    [JsonProperty("size")] public int Size { get; set; }

    [JsonProperty("openId")] public string OpenId { get; set; }

    [JsonProperty("firstFile")] public CowFile FirstFile { get; set; }

    [JsonProperty("firstFolder")] public object FirstFolder { get; set; }

    [JsonProperty("zipDownload")] public bool ZipDownload { get; set; }
}