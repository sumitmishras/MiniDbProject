namespace MiniDbProject.Models;

public class DatabaseInfo
{
    public int DatabaseId { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string? SourceServer { get; set; }
    public string? SourcePortal { get; set; }
    public long EstimatedSizeBytes { get; set; }
    public string EstimatedSizeDisplay => FormatSize(EstimatedSizeBytes);
    public int TableCount { get; set; }
    public bool IsSelected { get; set; }
    public bool IsValidForMigration { get; set; } = true;
    public string? ValidationMessage { get; set; }
    public string? ScriptFilePath { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public bool IsSystemDatabase { get; set; }

    private static string FormatSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < suffixes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {suffixes[order]}";
    }
}
