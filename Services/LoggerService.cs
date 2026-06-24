namespace BankingApplication.Services;

public class LoggerService
{
    private readonly string _logFilePath;

    public LoggerService(string logFilePath)
    {
        _logFilePath = logFilePath;
    }

    public void Log(string message)
    {
        try
        {
            string logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}";
            File.AppendAllText(_logFilePath, logLine);
        }
        catch
        {
            
        }
    }
}
