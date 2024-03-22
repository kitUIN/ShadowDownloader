using Avalonia.Controls;
using Avalonia.Interactivity;
using FluentAvalonia.UI.Controls;
using ShadowDownloader.UI.Converter;
using ShadowDownloader.UI.Models;
using ShadowDownloader.UI.ViewModels;

namespace ShadowDownloader.UI.Views;

public partial class MainWindow : Window
{
    public static MainWindow Current { get; private set; }

    public MainWindow()
    {
        InitializeComponent();
        MainWindowViewModel.TaskDialogShowed += OnTaskDialogShowed;
        Current = this;
    }

    private async void OnTaskDialogShowed(object? sender, string e)
    {
        await CheckFileTaskDialog.ShowAsync();
    }

    private void CheckFileTaskDialog_OnLoaded(object? sender, RoutedEventArgs e)
    {
        CheckFileTaskDialog.DataContext = this.DataContext;
    }

    private void CheckFileListBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            vm.CheckFileSelectedItems = ListNotNullConverter.CheckIList(CheckFileListBox.SelectedItems);
        }
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            if (sender is HyperlinkButton { Tag: ObservableCheckFileResult result })
            {
                vm.ReNameFile = result;
                vm.ReNameName = result.Name;
                vm.IsRename = true;
            }
        }
    }
}