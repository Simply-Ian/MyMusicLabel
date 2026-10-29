namespace MyMusicLabel.Infrastructure.Services.Logging;

internal static class LogFormatter
{
    public static string FormatInfo(string message, params object[] args)
    {
        var formatted = args.Length == 0 ? message : string.Format(message, args);
        return $"[{DateTime.Now.ToLocalTime()}] Info: {formatted}\n";
    }

    public static string FormatWarning(string message, params object[] args)
    {
        var formatted = args.Length == 0 ? message : string.Format(message, args);
        return $"[{DateTime.Now.ToLocalTime()}] Warning emitted: {formatted}\n";
    }

    public static string FormatWarning<TException>(string message, TException exc, params object[] args) where TException : Exception
    {
        var formatted = args.Length == 0 ? message : string.Format(message, args);
        return $"[{DateTime.Now.ToLocalTime()}] Warning emitted: {exc} \n{formatted}\n";
    }

    public static string FormatCritical<TException>(string message, TException exc, params object[] args) where TException : Exception
    {
        var formatted = args.Length == 0 ? message : string.Format(message, args);
        return $"[{DateTime.Now.ToLocalTime()}] Critical error encountered: {exc}\n{formatted}\n";
    }
}
