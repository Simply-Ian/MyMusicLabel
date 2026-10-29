namespace MyMusicLabel.Infrastructure.Services.Logging;

public class ConsoleCustomLogger : ICustomLogger
{
    public void LogInfo(string message, params object[] args) => Console.Write(LogFormatter.FormatInfo(message, args));

    public void LogWarning(string message, params object[] args) => Console.Write(LogFormatter.FormatWarning(message, args));

    public void LogWarning<TException>(string message, TException exc, params object[] args) where TException : Exception => 
        Console.Write(LogFormatter.FormatWarning(message, exc, args));

    public void LogCritical<TException>(string message, TException exc, params object[] args) where TException : Exception => 
        Console.Write(LogFormatter.FormatCritical(message, exc, args));
}
