using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Data.SqlClient;

namespace MiniDbWpf.Services;

public class DatabaseService : IDatabaseService
{
    private string BuildConnStr(string server, string database, string? user, string? password)
    {
        var b = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            TrustServerCertificate = true,
            ConnectTimeout = 10
        };
        if (!string.IsNullOrEmpty(user))
        { b.UserID = user; b.Password = password; }
        else
        { b.IntegratedSecurity = true; }
        return b.ConnectionString;
    }

    public async Task<(bool Success, string Message)> ValidateConnectionAsync(
        string server, string database, string? user, string? password)
    {
        try
        {
            using var conn = new SqlConnection(BuildConnStr(server, database, user, password));
            await conn.OpenAsync();
            return (true, "Connection successful.");
        }
        catch (Exception ex)
        {
            return (false, $"Connection failed: {ex.Message}");
        }
    }

    public async Task CreateDatabaseAsync(
        string sourceServer, string sourceDatabase, string? sourceUser, string? sourcePassword,
        string destinationServer, string destinationDatabase,
        DateTime fromDate, DateTime toDate, bool debug,
        IProgress<(int Percent, string Stage, string Task, string Detail)> progress,
        CancellationToken ct)
    {
        var destConnStr = BuildConnStr(destinationServer, "master", null, null);
        var tempDir = @"C:\Temp_MiniDB";
        var dacpacPath = Path.Combine(tempDir, "MiniDB.dacpac");
        Directory.CreateDirectory(tempDir);

        // Step 1: Create destination database
        progress.Report((5, "Setup", "Creating destination database...", $"CREATE DATABASE [{destinationDatabase}]"));
        ct.ThrowIfCancellationRequested();
        using (var conn = new SqlConnection(destConnStr))
        {
            await conn.OpenAsync(ct);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $@"
                IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = @db)
                BEGIN
                    CREATE DATABASE [{destinationDatabase}];
                    PRINT 'Database created.';
                END
                ELSE
                    PRINT 'Database already exists.';";
            cmd.Parameters.AddWithValue("@db", destinationDatabase);
            await cmd.ExecuteNonQueryAsync(ct);
        }
        progress.Report((15, "Setup", "Destination database ready.", ""));

        // Step 2: Extract schema from source via sqlpackage
        progress.Report((20, "Extract", "Extracting schema from source...", dacpacPath));
        ct.ThrowIfCancellationRequested();

        var extractArgs = new StringBuilder();
        extractArgs.Append("/Action:Extract ");
        extractArgs.Append($@"/SourceServerName:""{sourceServer}"" ");
        extractArgs.Append($@"/SourceDatabaseName:""{sourceDatabase}"" ");
        if (!string.IsNullOrEmpty(sourceUser))
        {
            extractArgs.Append($@"/SourceUser:""{sourceUser}"" ");
            extractArgs.Append($@"/SourcePassword:""{sourcePassword}"" ");
        }
        else
        {
            extractArgs.Append("/SourceIntegratedSecurity:true ");
        }
        extractArgs.Append("/SourceTrustServerCertificate:true ");
        extractArgs.Append($@"/TargetFile:""{dacpacPath}"" ");
        extractArgs.Append("/p:ExtractAllTableData=false");

        var extractResult = await RunSqlPackageAsync(extractArgs.ToString(), ct,
            msg => progress.Report((35, "Extract", "Extracting...", msg)));

        progress.Report((45, "Extract", "Schema extracted successfully.",
            extractResult.Take(200).ToString() ?? ""));

        // Step 3: Publish schema to destination via sqlpackage
        progress.Report((50, "Publish", "Publishing schema to destination...", ""));
        ct.ThrowIfCancellationRequested();

        var publishArgs = new StringBuilder();
        publishArgs.Append("/Action:Publish ");
        publishArgs.Append($@"/SourceFile:""{dacpacPath}"" ");
        publishArgs.Append($@"/TargetServerName:""{destinationServer}"" ");
        publishArgs.Append($@"/TargetDatabaseName:""{destinationDatabase}"" ");
        publishArgs.Append("/TargetTrustServerCertificate:true ");
        publishArgs.Append("/p:BlockOnPossibleDataLoss=false");

        var publishResult = await RunSqlPackageAsync(publishArgs.ToString(), ct,
            msg => progress.Report((65, "Publish", "Publishing...", msg)));

        progress.Report((75, "Publish", "Schema published to destination.", ""));

        // Step 4: Run createMiniDB procedure
        progress.Report((78, "Data Load", "Running createMiniDB procedure...",
            $"EXEC dbo.createMiniDB @SourceDB='{sourceDatabase}', @FromDate='{fromDate:yyyy-MM-dd}', @ToDate='{toDate:yyyy-MM-dd}', @Debug={(debug ? 0 : 1)}"));
        ct.ThrowIfCancellationRequested();

        var destDbConnStr = BuildConnStr(destinationServer, destinationDatabase, null, null);
        using (var conn = new SqlConnection(destDbConnStr))
        {
            await conn.OpenAsync(ct);
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "dbo.createMiniDB";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@SourceDB", sourceDatabase);
            cmd.Parameters.AddWithValue("@FromDate", fromDate);
            cmd.Parameters.AddWithValue("@ToDate", toDate);
            cmd.Parameters.AddWithValue("@Debug", debug ? 0 : 1);

            var output = new StringBuilder();
            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
                output.AppendLine(reader[0]?.ToString());

            progress.Report((90, "Data Load", "Data load complete.", output.ToString()));
        }

        // Step 5: Run checkMiniDBData
        progress.Report((95, "Validation", "Running checkMiniDBData...", ""));
        ct.ThrowIfCancellationRequested();

        var checkResult = await RunCheckDataAsync(destinationServer, destinationDatabase, null, null);
        progress.Report((100, "Complete", "MiniDB created successfully!", checkResult));
    }

    public async Task<string> RunCheckDataAsync(string server, string database, string? user, string? password)
    {
        var result = new StringBuilder();
        try
        {
            using var conn = new SqlConnection(BuildConnStr(server, database, user, password));
            await conn.OpenAsync();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "dbo.checkMiniDBData";
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var values = new object[reader.FieldCount];
                reader.GetValues(values);
                result.AppendLine(string.Join(" | ", values));
            }
        }
        catch (Exception ex)
        {
            result.AppendLine($"checkMiniDBData error: {ex.Message}");
        }
        return result.ToString();
    }

    private async Task<string> RunSqlPackageAsync(string arguments, CancellationToken ct,
        Action<string> onOutput)
    {
        var output = new StringBuilder();
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sqlpackage",
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                }
            };

            process.Start();
            var readTask = Task.Run(() =>
            {
                while (!process.StandardOutput.EndOfStream && !ct.IsCancellationRequested)
                {
                    var line = process.StandardOutput.ReadLine();
                    if (line != null)
                    {
                        output.AppendLine(line);
                        onOutput(line);
                    }
                }
                while (!process.StandardError.EndOfStream && !ct.IsCancellationRequested)
                {
                    var line = process.StandardError.ReadLine();
                    if (line != null)
                    {
                        output.AppendLine(line);
                        onOutput("[ERR] " + line);
                    }
                }
            }, ct);

            await Task.WhenAny(readTask, Task.Delay(-1, ct));
            if (!process.HasExited)
                process.Kill();
            process.WaitForExit(5000);
        }
        catch (Exception ex)
        {
            output.AppendLine($"sqlpackage error: {ex.Message}");
            onOutput($"Error: {ex.Message}");
        }
        return output.ToString();
    }
}
