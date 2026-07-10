namespace MiniDbProject.Utilities;

public static class TimeHelper
{
    public static string FormatTimeSpan(TimeSpan ts)
    {
        if (ts == TimeSpan.MaxValue) return "Calculating...";
        if (ts.TotalDays >= 1)
            return $"{(int)ts.TotalDays}d {ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
        if (ts.TotalHours >= 1)
            return $"{(int)ts.Hours}h {ts.Minutes}m {ts.Seconds}s";
        if (ts.TotalMinutes >= 1)
            return $"{(int)ts.Minutes}m {ts.Seconds}s";
        return $"{ts.Seconds}s";
    }

    public static string FormatDateTime(DateTime dt)
    {
        return dt.ToString("yyyy-MM-dd HH:mm:ss");
    }

    public static string FormatDate(DateTime dt)
    {
        return dt.ToString("yyyy-MM-dd");
    }

    public static string FormatElapsed(DateTime start)
    {
        return FormatTimeSpan(DateTime.Now - start);
    }

    public static string GetTimeAgo(DateTime dt)
    {
        var span = DateTime.Now - dt;
        if (span.TotalMinutes < 1) return "Just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays}d ago";
        return dt.ToString("MMM dd, yyyy");
    }
}
