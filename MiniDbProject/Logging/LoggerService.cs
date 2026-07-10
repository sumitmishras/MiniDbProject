using System.Text.Json;
using MiniDbProject.Configuration;
using MiniDbProject.Constants;
using MiniDbProject.Models;

namespace MiniDbProject.Logging;

public class LoggerService : ILoggerService, IDisposable
{
    private readonly List<LogEntry> _inMemoryLogs = new();
    private readonly string _logDirectory;
    private readonly string _logFilePattern;
    private readonly int _maxLogFiles;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private bool _disposed;

    public LoggerService()
    {
        var config = AppConfiguration.Load();
        _logDirectory = Path.Combine(Directory.GetCurrentDirectory(), config.Logging.LogDirectory);
        _logFilePattern = config.Logging.LogFileNamePattern;
        _maxLogFiles = config.Logging.MaxLogFiles;

        if (!Directory.Exists(_logDirectory))
            Directory.CreateDirectory(_logDirectory);

        CleanupOldLogFiles();
    }

    public async Task LogInfo(string message, string category = "General")
    {
        await WriteLogAsync(AppConstants.LogLevels.Information, message, category);
    }

    public async Task LogSuccess(string message, string category = "General")
    {
        await WriteLogAsync(AppConstants.LogLevels.Success, message, category);
    }

    public async Task LogWarning(string message, string category = "General")
    {
        await WriteLogAsync(AppConstants.LogLevels.Warning, message, category);
    }

    public async Task LogError(string message, Exception? ex = null, string category = "General")
    {
        await WriteLogAsync(AppConstants.LogLevels.Error, message, category, ex?.Message, ex?.StackTrace);
    }

    public async Task LogDebug(string message, string category = "General")
    {
        await WriteLogAsync(AppConstants.LogLevels.Debug, message, category);
    }

    public Task<List<LogEntry>> GetLogsAsync()
    {
        lock (_inMemoryLogs)
        {
            return Task.FromResult(_inMemoryLogs.OrderByDescending(l => l.Timestamp).ToList());
        }
    }

    public Task<List<LogEntry>> GetLogsByLevelAsync(string level)
    {
        lock (_inMemoryLogs)
        {
            return Task.FromResult(_inMemoryLogs
                .Where(l => l.Level.Equals(level, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(l => l.Timestamp)
                .ToList());
        }
    }

    public Task<List<LogEntry>> GetLogsByDateRangeAsync(DateTime from, DateTime to)
    {
        lock (_inMemoryLogs)
        {
            return Task.FromResult(_inMemoryLogs
                .Where(l => l.Timestamp >= from && l.Timestamp <= to)
                .OrderByDescending(l => l.Timestamp)
                .ToList());
        }
    }

    public Task<List<LogEntry>> SearchLogsAsync(string keyword)
    {
        lock (_inMemoryLogs)
        {
            return Task.FromResult(_inMemoryLogs
                .Where(l => l.Message.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            l.Category.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            l.Level.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(l => l.Timestamp)
                .ToList());
        }
    }

    public async Task ExportLogsAsync(string filePath)
    {
        var logs = await GetLogsAsync();
        var options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(logs, options);
        await File.WriteAllTextAsync(filePath, json);
    }

    public Task<int> GetLogCountAsync()
    {
        lock (_inMemoryLogs)
        {
            return Task.FromResult(_inMemoryLogs.Count);
        }
    }

    public Task ClearLogsAsync()
    {
        lock (_inMemoryLogs)
        {
            _inMemoryLogs.Clear();
        }
        return Task.CompletedTask;
    }

    private async Task WriteLogAsync(string level, string message, string category,
        string? exception = null, string? stackTrace = null)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Category = category,
            Message = message,
            Exception = exception,
            StackTrace = stackTrace,
            Username = Thread.CurrentPrincipal?.Identity?.Name
        };

        lock (_inMemoryLogs)
        {
            _inMemoryLogs.Add(entry);
        }

        await AppendToFileAsync(entry);
    }

    private async Task AppendToFileAsync(LogEntry entry)
    {
        await _semaphore.WaitAsync();
        try
        {
            string fileName = DateTime.Now.ToString(_logFilePattern);
            string filePath = Path.Combine(_logDirectory, fileName);
            string line = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.Level}] [{entry.Category}] {entry.Message}";
            if (!string.IsNullOrEmpty(entry.Exception))
                line += $" | Exception: {entry.Exception}";
            await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void CleanupOldLogFiles()
    {
        try
        {
            if (!Directory.Exists(_logDirectory)) return;
            var files = Directory.GetFiles(_logDirectory, "*.log")
                .OrderByDescending(f => f)
                .Skip(_maxLogFiles);
            foreach (var file in files)
            {
                try { File.Delete(file); } catch { }
            }
        }
        catch { }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _semaphore.Dispose();
            _disposed = true;
        }
    }
}
