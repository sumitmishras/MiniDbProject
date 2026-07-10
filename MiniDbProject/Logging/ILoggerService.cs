using MiniDbProject.Models;

namespace MiniDbProject.Logging;

public interface ILoggerService
{
    Task LogInfo(string message, string category = "General");
    Task LogSuccess(string message, string category = "General");
    Task LogWarning(string message, string category = "General");
    Task LogError(string message, Exception? ex = null, string category = "General");
    Task LogDebug(string message, string category = "General");
    Task<List<LogEntry>> GetLogsAsync();
    Task<List<LogEntry>> GetLogsByLevelAsync(string level);
    Task<List<LogEntry>> GetLogsByDateRangeAsync(DateTime from, DateTime to);
    Task<List<LogEntry>> SearchLogsAsync(string keyword);
    Task ExportLogsAsync(string filePath);
    Task<int> GetLogCountAsync();
    Task ClearLogsAsync();
}
