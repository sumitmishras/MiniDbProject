using MiniDbProject.DTOs;
using MiniDbProject.Models;

namespace MiniDbProject.Database;

public interface IDatabaseService
{
    Task<List<ServerPortal>> GetAvailablePortalsAsync();
    Task<List<DatabaseInfo>> GetDatabasesForPortalAsync(int portalId);
    Task<DatabaseInfo?> GetDatabaseDetailsAsync(int portalId, string databaseName);
    Task<bool> ValidateSourceDatabaseAsync(string serverName, string databaseName);
    Task<bool> ValidateDestinationAsync(string instanceName);
    Task<bool> ValidateScriptFileAsync(string scriptPath);
    Task<long> EstimateDatabaseSizeAsync(string scriptPath);
    Task<List<string>> GetDependenciesAsync(string scriptPath);
    Task<ProgressInfo> CreateDatabaseAsync(DatabaseSelectionDto selection, CancellationToken cancellationToken);
    string GetLocalInstanceName();
    int TotalSteps { get; }
}
