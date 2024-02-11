
using Newtonsoft.Json;
using ShadowDownloader.Response;

namespace ShadowDownloader;

public class Configuration
{
    private const string Name = "Configuration.json";
    /// <summary>
    /// 保存位置
    /// </summary>
    public string SavePath { get; set; }

    /// <summary>
    /// 下载线程数
    /// </summary>
    public int Parallel { get; set; }

    /// <summary>
    /// 最小块大小(单位:B)(默认:1MB)
    /// </summary>
    public long MinBlockSize { get; set; }

    /// <summary>
    /// 代理
    /// </summary>
    public string Proxies { get; set; }

    
    public int TaskId { get; set; }
    public Configuration(string savePath,int parallel,long minBlockSize,string proxies)
    {
        SavePath = savePath;
        Parallel = parallel;
        MinBlockSize = minBlockSize;
        Proxies = proxies;
        TaskId = 0;
    }

    public static Configuration Default()
    {
        return Load();
    }

    public void Save()
    {
        File.WriteAllText(Name,JsonConvert.SerializeObject(this));
    }
    public Task SaveAsync()
    {
        return File.WriteAllTextAsync(Name,JsonConvert.SerializeObject(this));
    }
    
    private static Configuration Load()
    {
        if (File.Exists(Name))
        {
            var res = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Name));
            if (res is not null)
            {
                return res;
            }
        }

        var downloadPath = Path.Combine(Directory.GetCurrentDirectory(), "downloads");
        if (!Directory.Exists(downloadPath)) Directory.CreateDirectory(downloadPath);
        return new Configuration(downloadPath, 
            10, 1L * 1024 * 1024, "");
    }
}