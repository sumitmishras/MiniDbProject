namespace MiniDbWpf.Services;

public interface IDatabaseService
{
    Task<(bool Success, string Message)> ValidateConnectionAsync(string server, string database, string? user, string? password);
    Task CreateDatabaseAsync(string sourceServer, string sourceDatabase, string? sourceUser, string? sourcePassword,
        string destinationServer, string destinationDatabase,
        DateTime fromDate, DateTime toDate, bool debug,
        IProgress<(int Percent, string Stage, string Task, string Detail)> progress,
        CancellationToken ct);
    Task<string> RunCheckDataAsync(string server, string database, string? user, string? password);
}
