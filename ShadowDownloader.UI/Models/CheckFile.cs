using ReactiveUI;
using ShadowDownloader.Response;

namespace ShadowDownloader.UI.Models;

public class CheckFile: ReactiveObject
{
    private string _format="";
    
    public string Format
    {
        get => _format;
        set => this.RaiseAndSetIfChanged(ref _format, value);
    }
    private string _name="";
    
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }
    private string _size="";
    
    public string Size
    {
        get => _size;
        set => this.RaiseAndSetIfChanged(ref _size, value);
    }
    /*private string _guid="";
    
    public string Guid
    {
        get => _guid;
        set => this.RaiseAndSetIfChanged(ref _guid, value);
    }*/
    private bool _canParallel;
    
    public bool CanParallel
    {
        get => _canParallel;
        set => this.RaiseAndSetIfChanged(ref _canParallel, value);
    }
    public CowFile File { get; }
    public string Link { get; }
    public CheckFile(CowFile f, bool canParallel,string link)
    {
        Format = f.FileInfo.Format;
        Name = $"{f.FileInfo.Title}.{f.FileInfo.Format}";
        Size = DownloadUtil.ConvertSize(f.FileInfo.Size);
        File = f;
        CanParallel = canParallel;
        Link = link;
    }

 
}