namespace MiniDbWpf.Services;

public interface IDatabaseService
{
    Task<bool> ValidateConnectionAsync(string server, string database);
    Task<bool> ValidateScriptAsync(string scriptPath);
    Task CreateDatabaseAsync(string server, string database, string scriptPath,
        IProgress<(int Percent, string Stage, string Task)> progress, CancellationToken ct);
}
