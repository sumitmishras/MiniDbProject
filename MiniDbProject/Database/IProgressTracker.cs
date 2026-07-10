using MiniDbProject.Models;

namespace MiniDbProject.Database;

public interface IProgressTracker
{
    event Action<ProgressInfo>? ProgressUpdated;
    event Action<string>? StageChanged;
    event Action<string>? TaskChanged;
    event Action<int>? PercentageChanged;
    void ReportProgress(ProgressInfo info);
    void ReportStage(string stage);
    void ReportTask(string task);
    void ReportPercentage(int percentage);
    void ReportStatus(string status);
    void ReportRecordsProcessed(long records);
    void Complete();
    void Fail(string errorMessage);
}

public class ProgressTracker : IProgressTracker
{
    public event Action<ProgressInfo>? ProgressUpdated;
    public event Action<string>? StageChanged;
    public event Action<string>? TaskChanged;
    public event Action<int>? PercentageChanged;

    public void ReportProgress(ProgressInfo info)
    {
        ProgressUpdated?.Invoke(info);
    }

    public void ReportStage(string stage)
    {
        StageChanged?.Invoke(stage);
    }

    public void ReportTask(string task)
    {
        TaskChanged?.Invoke(task);
    }

    public void ReportPercentage(int percentage)
    {
        PercentageChanged?.Invoke(percentage);
    }

    public void ReportStatus(string status)
    {
        StageChanged?.Invoke(status);
    }

    public void ReportRecordsProcessed(long records)
    {
    }

    public void Complete()
    {
        ReportStatus("Completed");
    }

    public void Fail(string errorMessage)
    {
        ReportStatus($"Failed: {errorMessage}");
    }
}
