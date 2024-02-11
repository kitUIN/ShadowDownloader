using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ShadowDownloader.UI.Models;
using ShadowDownloader.UI.ViewModels;
using ShadowDownloader.UI.Views;

namespace ShadowDownloader.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InitDb()
    {
        DbClient.InitDb();
        DbClient.Db.CodeFirst.InitTables<DbDownloadTask>();
    }
    public static ShadowDownloader Downloader { get; }= new ();
    public override void OnFrameworkInitializationCompleted()
    {
        Downloader.AddAdapter(new CowAdapter());
        InitDb();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}