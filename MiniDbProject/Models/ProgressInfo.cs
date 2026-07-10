namespace MiniDbProject.Models;

public class ProgressInfo
{
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
    public int Percentage => TotalSteps > 0 ? (int)((double)CompletedSteps / TotalSteps * 100) : 0;
    public string CurrentStage { get; set; } = string.Empty;
    public string CurrentTask { get; set; } = string.Empty;
    public string CurrentScript { get; set; } = string.Empty;
    public long RecordsProcessed { get; set; }
    public long TotalRecords { get; set; }
    public DateTime StartTime { get; set; } = DateTime.Now;
    public TimeSpan Elapsed => DateTime.Now - StartTime;
    public TimeSpan EstimatedRemaining
    {
        get
        {
            if (Percentage == 0) return TimeSpan.MaxValue;
            var totalEstimated = Elapsed.TotalMilliseconds / Percentage * 100;
            var remaining = totalEstimated - Elapsed.TotalMilliseconds;
            return remaining > 0 ? TimeSpan.FromMilliseconds(remaining) : TimeSpan.Zero;
        }
    }
    public string StatusMessage { get; set; } = "Initializing...";
    public bool IsRunning { get; set; }
    public bool IsPaused { get; set; }
    public bool IsCancelled { get; set; }
    public bool IsCompleted { get; set; }
    public List<string> CompletedTasks { get; set; } = new();
    public List<string> PendingTasks { get; set; } = new();
    public List<string> WarningMessages { get; set; } = new();
    public List<string> ErrorMessages { get; set; } = new();
}
