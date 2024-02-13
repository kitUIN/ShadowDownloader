namespace ShadowDownloader.Exception;

public class AdapterNotFoundException : System.Exception
{
    public AdapterNotFoundException(string? message) : base(message)
    {
    }

    public AdapterNotFoundException(string? message, System.Exception? innerException) : base(message, innerException)
    {
    }
}