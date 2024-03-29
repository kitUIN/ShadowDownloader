﻿using System.Collections.ObjectModel;
using ReactiveUI;
using ShadowDownloader.UI.Models;

namespace ShadowDownloader.UI.ViewModels;

public partial class MainWindowViewModel
{
    #region 检查文件窗口是否打开

    private bool _isOpenInCheckFile;

    /// <summary>
    /// 检查文件窗口是否打开
    /// </summary>
    public bool IsOpenInCheckFile
    {
        get => _isOpenInCheckFile;
        set => this.RaiseAndSetIfChanged(ref _isOpenInCheckFile, value);
    }

    #endregion

    #region 检查需要下载的文件列表

    private ObservableCollection<ObservableCheckFileResult> _checkFiles = new();

    /// <summary>
    /// 检查需要下载的文件列表
    /// </summary>
    public ObservableCollection<ObservableCheckFileResult> CheckFiles
    {
        get => _checkFiles;
        set => this.RaiseAndSetIfChanged(ref _checkFiles, value);
    }

    #endregion

    #region 文件下载任务列表

    private ObservableCollection<DownloadTask> _tasks = new();

    /// <summary>
    /// 文件下载任务列表
    /// </summary>
    public ObservableCollection<DownloadTask> Tasks
    {
        get => _tasks;
        set => this.RaiseAndSetIfChanged(ref _tasks, value);
    }

    #endregion

    #region 检查文件窗口中是否显示加载

    private bool _isVisibleInCheckFile;

    /// <summary>
    /// 检查文件窗口中是否显示加载
    /// </summary>
    public bool IsVisibleInCheckFile
    {
        get => _isVisibleInCheckFile;
        set => this.RaiseAndSetIfChanged(ref _isVisibleInCheckFile, value);
    }

    #endregion

    #region Dialog中的下载选中是否启用

    private bool _checkFileSelectedItems;

    /// <summary>
    ///  Dialog中的下载选中是否启用
    /// </summary>
    public bool CheckFileSelectedItems
    {
        get => _checkFileSelectedItems;
        set => this.RaiseAndSetIfChanged(ref _checkFileSelectedItems, value);
    }

    #endregion

    #region Dialog中的全部下载是否启用

    private bool _checkFileCount;

    /// <summary>
    ///  Dialog中的全部下载是否启用
    /// </summary>
    public bool CheckFileCount
    {
        get => _checkFileCount;
        set => this.RaiseAndSetIfChanged(ref _checkFileCount, value);
    }

    #endregion

    private bool _isRename;

    public bool IsRename
    {
        get => _isRename;
        set => this.RaiseAndSetIfChanged(ref _isRename, value);
    }

    private string _reNameOldName;

    public string ReNameName
    {
        get => _reNameOldName;
        set => this.RaiseAndSetIfChanged(ref _reNameOldName, value);
    }

    private string _currentId = "";
    public ObservableCheckFileResult ReNameFile { get; set; }
}