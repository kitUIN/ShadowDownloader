using System.ComponentModel;
using ShadowDownloader.Model;

namespace ShadowDownloader.UI.Models;

public class ObservableCheckFileResult : CheckFileResult, INotifyPropertyChanged
{
    private string _showName;

    public string ShowName
    {
        get => _showName;
        set
        {
            _showName = value;
            OnPropertyChanged(nameof(ShowName));
            Name = _showName;
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ObservableCheckFileResult(string id, string name, string link, string path, bool canParallel, long size,
        object? extra = null) : base(id, name, link, path, canParallel, size, extra)
    {
        ShowName = name;
    }
}