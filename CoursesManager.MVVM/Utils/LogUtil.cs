using System.Diagnostics;
using System.IO;

public class LogUtil
{
    private static readonly string LogFilePath;
    private static readonly object _lock = new();

    static LogUtil()
    {
        string logDirectory;

#if DEBUG
        logDirectory = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName ?? "");
#else
         logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
#endif

        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        LogFilePath = Path.Combine(logDirectory, "application.log");
    }

    public static void Log(string message, LogLevel level = LogLevel.Info)
    {
        string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

        try
        {
            // Lock is implemented to ensure thread safety
            lock (_lock)
            {
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"Failed to write log to file: {exception.Message}");
        }
    }

    public static void Info(string message) => Log(message, LogLevel.Info);
    public static void Warning(string message) => Log(message, LogLevel.Warning);
    public static void Error(string message) => Log(message, LogLevel.Error);

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }
}
