using ReactiveUI;

namespace ShadowDownloader.UI.ViewModels;

public partial class SettingWindowViewModel
{
    #region 文件下载任务列表

    private string _savePath = App.Downloader.Config.SavePath;

    /// <summary>
    /// 文件下载任务列表
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
}