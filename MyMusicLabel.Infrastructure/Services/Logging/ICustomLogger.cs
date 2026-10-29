namespace MyMusicLabel.Infrastructure.Services.Logging;

public interface ICustomLogger
{
    void LogInfo(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogWarning<TException>(string message, TException exc, params object[] args) where TException : Exception;
    void LogCritical<TException>(string message, TException exc, params object[] args) where TException : Exception;
}