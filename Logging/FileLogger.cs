namespace MeteredPDUWebNew.Logging;

public class FileLogger : ILogger
{
    private static readonly object _lock = new object();
    private const int FileWriteAttempts = 1000;
        
    private string _path;

    public FileLogger(string path) => _path = path;

    public bool IsEnabled(LogLevel logLevel) => true;
    public IDisposable BeginScope<TState>(TState state) => null;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        if (formatter == null)
            return;
            
        lock (_lock)
        {
            int attempts = 0;
            bool writeSuccess = false;
            while (!writeSuccess && attempts < FileWriteAttempts)
            {
                try
                {
                    File.AppendAllText(_path, $"[{logLevel}] [{DateTime.Now:dd.MM.yyyy hh:mm:ss}]" + formatter(state, exception) + Environment.NewLine);
                    writeSuccess = true;
                }
                catch
                {

                }

                attempts++;
            }
        }
    }
}