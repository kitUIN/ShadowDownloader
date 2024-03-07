using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using ReactiveUI;
using ShadowDownloader.UI.Views;

namespace ShadowDownloader.UI.ViewModels;

public partial class SettingWindowViewModel
{
    public ReactiveCommand<Unit, Unit> SetSavePathCommand =>
        ReactiveCommand.CreateFromTask(SetSavePath);

    private async Task SetSavePath()
    {
        var storageProvider = SettingWindow.Current.StorageProvider;
        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "选择下载文件夹",
            AllowMultiple = false
        });

        if (folders.Count >= 1)
        {
            SavePath = folders[0].Path.ToString()[8..].Replace("/", "\\");
        }
    }
}