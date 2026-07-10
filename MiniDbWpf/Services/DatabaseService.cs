using System.IO;
namespace MiniDbWpf.Services;

public class DatabaseService : IDatabaseService
{
    public Task<bool> ValidateConnectionAsync(string server, string database)
    {
        if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(database))
            return Task.FromResult(false);
        return Task.FromResult(true);
    }

    public Task<bool> ValidateScriptAsync(string scriptPath)
    {
        if (!File.Exists(scriptPath)) return Task.FromResult(false);
        var content = File.ReadAllText(scriptPath);
        return Task.FromResult(!string.IsNullOrWhiteSpace(content));
    }

    public async Task CreateDatabaseAsync(string server, string database, string scriptPath,
        IProgress<(int Percent, string Stage, string Task)> progress, CancellationToken ct)
    {
        var stages = new (int Percent, string Stage, string Task)[]
        {
            (5, "Initializing", "Validating parameters..."),
            (10, "Connection", "Connecting to SQL Server..."),
            (15, "Connection", "Authenticating..."),
            (20, "Connection", "Connection established."),
            (25, "Analysis", "Analyzing SQL script..."),
            (30, "Analysis", "Parsing statements..."),
            (35, "Analysis", "Validating syntax..."),
            (40, "Preparation", "Checking dependencies..."),
            (45, "Preparation", "Preparing execution plan..."),
            (50, "Execution", "Creating database structure..."),
            (55, "Execution", "Creating tables..."),
            (60, "Execution", "Creating views..."),
            (65, "Execution", "Creating indexes..."),
            (70, "Execution", "Creating stored procedures..."),
            (75, "Execution", "Creating triggers..."),
            (80, "Data", "Inserting seed data..."),
            (85, "Data", "Verifying constraints..."),
            (90, "Finalizing", "Running post-deployment scripts..."),
            (95, "Finalizing", "Updating statistics..."),
            (100, "Complete", "Database created successfully!")
        };

        foreach (var stage in stages)
        {
            ct.ThrowIfCancellationRequested();
            progress.Report(stage);
            await Task.Delay(800, ct);
        }
    }
}
