namespace MiniDbProject.Models;

public class ServerPortal
{
    public int PortalId { get; set; }
    public string PortalName { get; set; } = string.Empty;
    public string ServerName { get; set; } = string.Empty;
    public string ServerType { get; set; } = "SQL Server";
    public bool IsAccessible { get; set; } = true;
    public string? Description { get; set; }
    public List<DatabaseInfo> Databases { get; set; } = new();
}
