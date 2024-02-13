using Avalonia.Controls;
using Avalonia.Interactivity;
using ShadowDownloader.UI.ViewModels;

namespace ShadowDownloader.UI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        MainWindowViewModel.TaskDialogShowed += OnTaskDialogShowed;
    }

    private async void OnTaskDialogShowed(object? sender, string e)
    {
        await CheckFileTaskDialog.ShowAsync();
    }

    private void CheckFileTaskDialog_OnLoaded(object? sender, RoutedEventArgs e)
    {
        CheckFileTaskDialog.DataContext = this.DataContext;
    }
}