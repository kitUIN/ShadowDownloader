using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
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
        };
        await ContentDialogShowAsync(dialog);
    }
}