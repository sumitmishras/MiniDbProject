using MiniDbWpf.Models;

namespace MiniDbWpf.Services;

public interface ILoggerService
{
    Task LogInfo(string message, string category = "System");
    Task LogSuccess(string message, string category = "System");
    Task LogWarning(string message, string category = "System");
    Task LogError(string message, string category = "System");
    Task<List<LogEntry>> GetLogsAsync();
}
