using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ShadowDownloader.UI.Models;
using ShadowDownloader.UI.Views;

namespace ShadowDownloader.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public static CowDownloader Downloader = new();

    public async Task<ContentDialogResult> ContentDialogShowAsync(ContentDialog dialog)
    {
        var dialogResult = await dialog.ShowAsync();
        return dialogResult;
    }
    
    private double _progressValue = 0;

    public double ProgressValue
    {
        get => _progressValue;
        set => this.RaiseAndSetIfChanged(ref _progressValue, value);
    }

    #region 检查文件窗口是否打开
    private bool _isOpen;

    public bool IsOpen
    {
        get => _isOpen;
        set => this.RaiseAndSetIfChanged(ref _isOpen, value);
    }
    #endregion
    
    #region 检查需要下载的文件
    private ObservableCollection<CheckFile> _checkFiles = new();

    public ObservableCollection<CheckFile> CheckFiles
    {
        get => _checkFiles;
        set => this.RaiseAndSetIfChanged(ref _checkFiles, value);
    }
    #endregion

    #region 检查文件窗口中是否显示加载
    private bool _isVisibleInCheckFile;

    public bool IsVisibleInCheckFile
    {
        get => _isVisibleInCheckFile;
        set => this.RaiseAndSetIfChanged(ref _isVisibleInCheckFile, value);
    }
    #endregion
    
    private string _speedValue = "0 MB/s";

    public string SpeedValue
    {
        get => _speedValue;
        set => this.RaiseAndSetIfChanged(ref _speedValue, value);
    }

    public ReactiveCommand<string, Unit> ShowAddUrlCommand =>
        ReactiveCommand.CreateFromTask<string>(ShowAddUrlAsync);

    private async Task ShowAddUrlAsync(string label)
    {
        var text = new TextBox()
        {
            MaxWidth = 280,
            MaxLines = 6,
            TextWrapping = TextWrapping.Wrap,
            Watermark = "输入你要下载的地址"
        };
        var dialog = new ContentDialog
        {
            Title = label,
            PrimaryButtonText = "确定",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = "取消",
            Content = text,
        };
        dialog.PrimaryButtonClick += async (sender, args) =>
        {
            if (sender.Content is not TextBox { Text: string url }) return;
            var id = App.Downloader.GetId(url);
            if (id != null)
            {
                IsOpen = true;
                IsVisibleInCheckFile = true;
                await CheckFilesInit(id);
            }
        };
        await ContentDialogShowAsync(dialog);
    }
    private async Task CheckFilesInit(string id)
    {
        CheckFiles.Clear();
        var (flag, message, guid) = await App.Downloader.Fetch(id);
        if (flag)
        {
            var files = await App.Downloader.FetchItems(guid);
            if (files.CheckCode())
            {
                foreach (var f in files.Data.Files)
                {
                    IsVisibleInCheckFile = false;
                    CheckFiles.Add(new CheckFile(f));
                }
            }
        }
    }
}