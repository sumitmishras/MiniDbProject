using System.Collections.Concurrent;
using System.IO;
using MiniDbWpf.Models;
using Serilog;

namespace MiniDbWpf.Services;

public class LoggerService : ILoggerService, IDisposable
{
    private readonly ConcurrentBag<LogEntry> _logs = new();
    private readonly Serilog.Core.Logger _serilog;

    public LoggerService()
    {
        var logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        Directory.CreateDirectory(logDir);

        _serilog = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(logDir, "app-.log"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}")
            .MinimumLevel.Information()
            .CreateLogger();
    }

    public Task LogInfo(string message, string category = "System")
        => AddLog("Info", message, category);

    public Task LogSuccess(string message, string category = "System")
        => AddLog("Success", message, category);

    public Task LogWarning(string message, string category = "System")
        => AddLog("Warning", message, category);

    public Task LogError(string message, string category = "System")
        => AddLog("Error", message, category);

    private Task AddLog(string level, string message, string category)
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = level,
            Message = message,
            Category = category
        };
        _logs.Add(entry);

        switch (level)
        {
            case "Info": _serilog.Information("{Category}: {Message}", category, message); break;
            case "Success": _serilog.Information("[SUCCESS] {Category}: {Message}", category, message); break;
            case "Warning": _serilog.Warning("{Category}: {Message}", category, message); break;
            case "Error": _serilog.Error("{Category}: {Message}", category, message); break;
        }
        return Task.CompletedTask;
    }

    public Task<List<LogEntry>> GetLogsAsync()
    {
        var sorted = _logs.OrderByDescending(l => l.Timestamp).ToList();
        return Task.FromResult(sorted);
    }

    public void Dispose() => _serilog?.Dispose();
}
