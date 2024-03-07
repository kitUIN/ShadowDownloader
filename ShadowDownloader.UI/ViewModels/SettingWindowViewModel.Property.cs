using System;
using ReactiveUI;

namespace ShadowDownloader.UI.ViewModels;

public partial class SettingWindowViewModel
{
    #region 代理

    private string? _proxies = App.Downloader.Config.Proxies;

    /// <summary>
    /// 代理
    /// </summary>
    public string? Proxies
    {
        get => _proxies;
        set
        {
            this.RaiseAndSetIfChanged(ref _proxies, value);
            try
            {
                App.Downloader.Config.Proxies = _proxies;
                App.Downloader.Config.SaveAsync();
            }
            catch (UriFormatException)
            {
                /*throw new Exception("无效的地址");*/
            }
        }
    }

    #endregion

    #region 文件下载路径

    private string _savePath = App.Downloader.Config.SavePath;

    /// <summary>
    /// 文件下载路径
    /// </summary>
    public string SavePath
    {
        get => _savePath;
        set
        {
            this.RaiseAndSetIfChanged(ref _savePath, value);
            App.Downloader.Config.SavePath = _savePath;
            App.Downloader.Config.SaveAsync();
        }
    }

    #endregion

    #region 线程数量

    private int _parallel = App.Downloader.Config.Parallel;

    /// <summary>
    /// 线程数量
    /// </summary>
    public int Parallel
    {
        get => _parallel;
        set
        {
            this.RaiseAndSetIfChanged(ref _parallel, value);
            App.Downloader.Config.Parallel = _parallel;
            App.Downloader.Config.SaveAsync();
        }
    }

    #endregion
}