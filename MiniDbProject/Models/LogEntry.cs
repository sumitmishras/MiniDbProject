namespace MiniDbProject.Models;

public class LogEntry
{
    public int LogId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string Level { get; set; } = Constants.AppConstants.LogLevels.Information;
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Exception { get; set; }
    public string? StackTrace { get; set; }
    public long? ExecutionTimeMs { get; set; }
}
