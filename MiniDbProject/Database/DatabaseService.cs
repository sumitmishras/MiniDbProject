using MiniDbProject.DTOs;
using MiniDbProject.Helpers;
using MiniDbProject.Logging;
using MiniDbProject.Models;

namespace MiniDbProject.Database;

public class DatabaseService : IDatabaseService
{
    private readonly ILoggerService _logger;
    private readonly Random _random = new();

    public int TotalSteps => 8;

    public DatabaseService(ILoggerService logger)
    {
        _logger = logger;
    }

    public async Task<List<ServerPortal>> GetAvailablePortalsAsync()
    {
        await _logger.LogInfo("Fetching available server portals...");
        await Task.Delay(200);

        return new List<ServerPortal>
        {
            new()
            {
                PortalId = 1,
                PortalName = "Production Portal",
                ServerName = "PROD-SQL-01",
                ServerType = "SQL Server 2022",
                Description = "Production environment with live data",
                Databases = new List<DatabaseInfo>
                {
                    new() { DatabaseId = 1, DatabaseName = "ERP_System", EstimatedSizeBytes = 2_500_000_000, TableCount = 145, SourceServer = "PROD-SQL-01", SourcePortal = "Production Portal" },
                    new() { DatabaseId = 2, DatabaseName = "CRM_Database", EstimatedSizeBytes = 850_000_000, TableCount = 67, SourceServer = "PROD-SQL-01", SourcePortal = "Production Portal" },
                    new() { DatabaseId = 3, DatabaseName = "HR_Management", EstimatedSizeBytes = 320_000_000, TableCount = 34, SourceServer = "PROD-SQL-01", SourcePortal = "Production Portal" }
                }
            },
            new()
            {
                PortalId = 2,
                PortalName = "Development Portal",
                ServerName = "DEV-SQL-02",
                ServerType = "SQL Server 2022",
                Description = "Development and testing environment",
                Databases = new List<DatabaseInfo>
                {
                    new() { DatabaseId = 4, DatabaseName = "Dev_ERP_System", EstimatedSizeBytes = 500_000_000, TableCount = 120, SourceServer = "DEV-SQL-02", SourcePortal = "Development Portal" },
                    new() { DatabaseId = 5, DatabaseName = "Dev_CRM_Database", EstimatedSizeBytes = 200_000_000, TableCount = 55, SourceServer = "DEV-SQL-02", SourcePortal = "Development Portal" },
                    new() { DatabaseId = 6, DatabaseName = "Test_Inventory", EstimatedSizeBytes = 150_000_000, TableCount = 40, SourceServer = "DEV-SQL-02", SourcePortal = "Development Portal" },
                    new() { DatabaseId = 7, DatabaseName = "Dev_Reporting", EstimatedSizeBytes = 80_000_000, TableCount = 25, SourceServer = "DEV-SQL-02", SourcePortal = "Development Portal" }
                }
            },
            new()
            {
                PortalId = 3,
                PortalName = "Staging Portal",
                ServerName = "STG-SQL-03",
                ServerType = "SQL Server 2019",
                Description = "Staging environment for pre-production validation",
                Databases = new List<DatabaseInfo>
                {
                    new() { DatabaseId = 8, DatabaseName = "Staging_ERP", EstimatedSizeBytes = 1_800_000_000, TableCount = 140, SourceServer = "STG-SQL-03", SourcePortal = "Staging Portal" },
                    new() { DatabaseId = 9, DatabaseName = "Staging_WebApp", EstimatedSizeBytes = 400_000_000, TableCount = 45, SourceServer = "STG-SQL-03", SourcePortal = "Staging Portal" }
                }
            },
            new()
            {
                PortalId = 4,
                PortalName = "Analytics Portal",
                ServerName = "ANL-SQL-04",
                ServerType = "SQL Server 2022",
                Description = "Analytics and reporting environment",
                Databases = new List<DatabaseInfo>
                {
                    new() { DatabaseId = 10, DatabaseName = "DataWarehouse", EstimatedSizeBytes = 10_000_000_000, TableCount = 200, SourceServer = "ANL-SQL-04", SourcePortal = "Analytics Portal" },
                    new() { DatabaseId = 11, DatabaseName = "BI_Reports", EstimatedSizeBytes = 2_100_000_000, TableCount = 85, SourceServer = "ANL-SQL-04", SourcePortal = "Analytics Portal" }
                }
            }
        };
    }

    public async Task<List<DatabaseInfo>> GetDatabasesForPortalAsync(int portalId)
    {
        await _logger.LogInfo($"Fetching databases for portal ID: {portalId}");
        await Task.Delay(100);
        var portals = await GetAvailablePortalsAsync();
        return portals.FirstOrDefault(p => p.PortalId == portalId)?.Databases ?? new List<DatabaseInfo>();
    }

    public async Task<DatabaseInfo?> GetDatabaseDetailsAsync(int portalId, string databaseName)
    {
        await _logger.LogInfo($"Fetching details for database '{databaseName}'");
        await Task.Delay(100);
        var databases = await GetDatabasesForPortalAsync(portalId);
        return databases.FirstOrDefault(d => d.DatabaseName == databaseName);
    }

    public async Task<bool> ValidateSourceDatabaseAsync(string serverName, string databaseName)
    {
        await _logger.LogInfo($"Validating source database '{databaseName}' on '{serverName}'");
        await Task.Delay(200);
        return !string.IsNullOrWhiteSpace(serverName) && !string.IsNullOrWhiteSpace(databaseName);
    }

    public async Task<bool> ValidateDestinationAsync(string instanceName)
    {
        await _logger.LogInfo($"Validating destination instance: {instanceName}");
        await Task.Delay(150);

        if (string.IsNullOrWhiteSpace(instanceName))
        {
            await _logger.LogWarning("Empty destination instance name provided.");
            return false;
        }

        bool isLocal = ValidationHelper.IsLocalSqlInstance(instanceName);
        if (!isLocal)
        {
            await _logger.LogError($"Remote destination instance rejected: {instanceName}");
        }
        return isLocal;
    }

    public async Task<bool> ValidateScriptFileAsync(string scriptPath)
    {
        await _logger.LogInfo($"Validating script file: {scriptPath}");
        await Task.Delay(100);
        return ValidationHelper.IsValidFilePath(scriptPath);
    }

    public async Task<long> EstimateDatabaseSizeAsync(string scriptPath)
    {
        await _logger.LogInfo($"Estimating database size from script: {scriptPath}");
        await Task.Delay(300);
        return (long)(_random.NextDouble() * 500_000_000 + 50_000_000);
    }

    public async Task<List<string>> GetDependenciesAsync(string scriptPath)
    {
        await _logger.LogInfo($"Analyzing dependencies for script: {scriptPath}");
        await Task.Delay(200);
        return new List<string>
        {
            "SQL Server LocalDB or higher",
            "Minimum 2 GB free disk space",
            ".NET Framework 4.8+",
            "SQL Server Management Objects (SMO) - Optional"
        };
    }

    public async Task<ProgressInfo> CreateDatabaseAsync(DatabaseSelectionDto selection, CancellationToken cancellationToken)
    {
        await _logger.LogInfo($"Starting database creation process for '{selection.SourceDatabaseName}'");
        var progress = new ProgressInfo
        {
            TotalSteps = TotalSteps,
            IsRunning = true,
            StartTime = DateTime.Now,
            PendingTasks = new List<string>
            {
                "Validating source database connection",
                "Parsing SQL script",
                "Creating database structure",
                "Creating tables",
                "Creating indexes and constraints",
                "Inserting reference data",
                "Running post-creation validations",
                "Finalizing database"
            }
        };

        string[] taskNames = progress.PendingTasks.ToArray();

        for (int i = 0; i < TotalSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            progress.CurrentStage = $"Step {i + 1} of {TotalSteps}";
            progress.CurrentTask = taskNames[i];
            progress.CurrentScript = $"executing_step_{i + 1}.sql";
            progress.StatusMessage = $"Processing: {taskNames[i]}...";

            await _logger.LogInfo($"Database creation - {progress.CurrentStage}: {progress.CurrentTask}");

            int stepDuration = _random.Next(800, 2000);
            int subSteps = _random.Next(3, 8);
            for (int s = 0; s < subSteps; s++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(stepDuration / subSteps, cancellationToken);
                progress.RecordsProcessed += _random.Next(10, 100);
                progress.CompletedSteps = i;
            }

            progress.CompletedSteps = i + 1;
            progress.CompletedTasks.Add(taskNames[i]);
        }

        progress.IsRunning = false;
        progress.IsCompleted = true;
        progress.StatusMessage = "Database created successfully!";

        await _logger.LogSuccess($"Database '{selection.SourceDatabaseName}' created successfully on local instance.");
        return progress;
    }

    public string GetLocalInstanceName()
    {
        return "(localdb)\\MSSQLLocalDB";
    }
}
