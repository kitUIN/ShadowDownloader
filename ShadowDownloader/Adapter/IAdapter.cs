using ShadowDownloader.Model;

namespace ShadowDownloader.Adapter;

public interface IAdapter
{
    /// <summary>
    /// 适配器Id
    /// </summary>
    public string GetId();

    
    
    public CheckUrlResult CheckUrl(string url);

    /// <summary>
    /// 检查下载链接,提取出下载内容
    /// </summary>
    /// <param name="result">CheckUrlResult</param>
    /// <param name="savePath">保存位置</param>
    /// <returns>列表</returns>
    public Task<List<CheckFileResult>> CheckFile(CheckUrlResult result, string savePath);

    public Task<int> Download(CheckFileResult result, Configuration config);
}