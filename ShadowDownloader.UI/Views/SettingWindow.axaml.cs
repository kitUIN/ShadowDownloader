using Avalonia.Controls;

namespace ShadowDownloader.UI.Views;

public partial class SettingWindow : Window
{
    public static SettingWindow Current { get; private set; }

    public SettingWindow()
    {
        InitializeComponent();
        Current = this;
    }
}