using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using ReactiveUI;
using SqlSugar;

namespace ShadowDownloader.UI.Models;

public class DownloadTask: ReactiveObject
{
    private int _id;
    
    public int Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }
    private string _name = "";
    
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
    
    private int _parallel;
    
    public int Parallel
    {
        get => _parallel;
        set => this.RaiseAndSetIfChanged(ref _parallel, value);
    }
    private double _percent;
    
    public double Percent
    {
        get => _percent;
        set => this.RaiseAndSetIfChanged(ref _percent, value);
    }
    
    public long Size { get; }

    private long _received;
    
    public long Received
    {
        get => _received;
        set => this.RaiseAndSetIfChanged(ref _received, value);
    }
    private long _speed;
    
    public long Speed
    {
        get => _speed;
        set => this.RaiseAndSetIfChanged(ref _speed, value);
    }
    private ObservableCollection<DownloadTask> _siblings = new();

    public ObservableCollection<DownloadTask> Siblings
    {
        get => _siblings;
        set => this.RaiseAndSetIfChanged(ref _siblings, value);
    }

    public DownloadTask(int id, string name, long size)
    {
        Id = id;
        Name = name;
        Size = size;
        Parallel = 0;
    }

    public void Append(DownloadTask task)
    {
        Siblings.Add(task);
        Parallel = Siblings.Count > 0 ? Siblings.Count : 1;
    }

    public async Task SaveDbAsync()
    {
        await DbClient.Db.Storageable(new DbDownloadTask
        {
            Id = Id,
            Name = Name,
            Parallel = Parallel,
            Percent = Percent,
            Received = Received,
            Size = Size,
        }).ExecuteCommandAsync();
    }
}