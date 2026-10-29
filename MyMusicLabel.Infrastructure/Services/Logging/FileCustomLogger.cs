namespace MyMusicLabel.Infrastructure.Services.Logging;

public class FileCustomLogger(string filename) : ICustomLogger
{
    public void LogInfo(string message, params object[] args)
    {
        // TODO: создавать новый файл?
        using var stream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var writer = new StreamWriter(stream);
        writer.Write(LogFormatter.FormatInfo(message, args));
    }

    public void LogWarning(string message, params object[] args)
    {
        using var stream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var writer = new StreamWriter(stream);
        writer.Write(LogFormatter.FormatWarning(message, args));
    }

    public void LogWarning<TException>(string message, TException exc, params object[] args) where TException : Exception
    {
        using var stream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var writer = new StreamWriter(stream);
        writer.Write(LogFormatter.FormatWarning(message, exc, args));
    }

    public void LogCritical<TException>(string message, TException exc, params object[] args) where TException : Exception
    {
        using var stream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Read);
        using var writer = new StreamWriter(stream);
        writer.Write(LogFormatter.FormatCritical(message, exc, args));
    }
}
