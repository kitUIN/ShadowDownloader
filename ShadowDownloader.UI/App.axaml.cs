using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Serilog;
using ShadowDownloader.Adapter;
using ShadowDownloader.UI.Models;
using ShadowDownloader.UI.ViewModels;
using ShadowDownloader.UI.Views;

namespace ShadowDownloader.UI;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File("logs/log-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    private void InitDb()
    {
        DbClient.InitDb();
        DbClient.Db.CodeFirst.InitTables<DbDownloadTask>();
        DbClient.Db.CodeFirst.InitTables<DbParallelDownloadTask>();
    }

    public static ShadowDownloader Downloader { get; } = new();

    public override void OnFrameworkInitializationCompleted()
    {
        Downloader.AddAdapter(new CowAdapter());
        Downloader.AddAdapter(new UrlAdapter());
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