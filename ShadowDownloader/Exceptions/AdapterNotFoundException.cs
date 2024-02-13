namespace ShadowDownloader.Exceptions;

public class AdapterNotFoundException : Exception
{
    public AdapterNotFoundException(string? message) : base(message)
    {
    }

    public AdapterNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}