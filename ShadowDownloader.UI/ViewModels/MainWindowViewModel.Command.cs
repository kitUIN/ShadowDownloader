﻿using System.Collections;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using ReactiveUI;
using ShadowDownloader.Model;
using ShadowDownloader.UI.Extension;
using ShadowDownloader.UI.Views;

namespace ShadowDownloader.UI.ViewModels;

public partial class MainWindowViewModel
{
    public ReactiveCommand<string, Unit> ShowAddUrlCommand =>
        ReactiveCommand.CreateFromTask<string>(ShowAddUrlDialogAsync);

    /// <summary>
    /// 添加下载解析窗口
    /// </summary>
    /// <param name="id">id</param>
    private async Task ShowAddUrlDialogAsync(string id)
    {
        _currentId = id;
        var text = new TextBox()
        {
            MaxWidth = 280,
            MaxLines = 6,
            TextWrapping = TextWrapping.Wrap,
            Watermark = "输入你要下载的地址"
        };
        var dialog = new ContentDialog
        {
            Title = App.Downloader.GetAdapter(id).GetName(),
            PrimaryButtonText = "确定",
            DefaultButton = ContentDialogButton.Primary,
            IsPrimaryButtonEnabled = true,
            IsSecondaryButtonEnabled = true,
            SecondaryButtonText = "取消",
            Content = text,
        };
        dialog.PrimaryButtonClick += async (sender, args) =>
        {
            if (sender.Content is not TextBox { Text: { } url }) return;
            var res = App.Downloader.CheckUrl(id, url);
            if (res.Success)
            {
                TaskDialogShowAsync("CheckFileTaskDialog");
                IsVisibleInCheckFile = true;
                await CheckFilesInit(id, res);
                IsVisibleInCheckFile = false;
            }
        };
        await dialog.ShowAsync();
    }

    public ReactiveCommand<Unit, Unit> RenameCommand =>
        ReactiveCommand.Create(Rename);

    private void Rename()
    {
        ReNameFile.ShowName = ReNameName;
        IsRename = false;
    }

    public ReactiveCommand<Unit, Unit> DownloadAllCommand =>
        ReactiveCommand.CreateFromTask(DownloadAllFileAsync);

    /// <summary>
    /// TaskDialog 中的 下载全部
    /// </summary>
    private async Task DownloadAllFileAsync()
    {
        IsOpenInCheckFile = false;
        foreach (var checkFile in CheckFiles)
        {
            var taskRecord = await App.Downloader.Download(_currentId, checkFile);
            InitTask(taskRecord, _currentId);
            taskRecord.ScheduleTasks.StartAll();
        }

        _currentId = "";
    }

    public ReactiveCommand<IList?, Unit> DownloadSelectedCommand =>
        ReactiveCommand.CreateFromTask<IList?>(DownloadSelectedFileAsync);

    /// <summary>
    /// TaskDialog 中的 下载选中
    /// </summary>
    private async Task DownloadSelectedFileAsync(IList? list)
    {
        if (list is null) return;
        IsOpenInCheckFile = false;
        foreach (CheckFileResult checkFile in list)
        {
            var taskRecord = await App.Downloader.Download(_currentId, checkFile);
            InitTask(taskRecord, _currentId);
            taskRecord.ScheduleTasks.StartAll();
        }

        _currentId = "";
    }

    public ReactiveCommand<Unit, Unit> OpenSavePathCommand =>
        ReactiveCommand.Create(OpenSavePath);

    private static void OpenSavePath()
    {
        System.Diagnostics.Process.Start("Explorer.exe", App.Downloader.Config.SavePath);
    }

    public ReactiveCommand<Unit, Unit> OpenSettingCommand =>
        ReactiveCommand.Create(OpenSetting);

    private static void OpenSetting()
    {
        var settingWindow = new SettingWindow()
        {
            DataContext = new SettingWindowViewModel()
        };
        settingWindow.Show();
    }
}