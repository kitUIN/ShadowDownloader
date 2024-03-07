using Avalonia.Controls;
using Avalonia.Interactivity;
using ShadowDownloader.UI.Converter;
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
}