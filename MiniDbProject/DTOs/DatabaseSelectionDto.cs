namespace MiniDbProject.DTOs;

public class DatabaseSelectionDto
{
    public int PortalId { get; set; }
    public string PortalName { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
    public string SourceDatabaseName { get; set; } = string.Empty;
    public string DestinationInstance { get; set; } = string.Empty;
    public string DestinationDatabaseName { get; set; } = string.Empty;
    public string? ScriptFilePath { get; set; }
    public bool IsLocalDestination { get; set; }
}
