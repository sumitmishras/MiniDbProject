using MiniDbProject.Constants;
using MiniDbProject.Helpers;
using MiniDbProject.Logging;
using MiniDbProject.Models;
using MiniDbProject.Resources;

namespace MiniDbProject.Views;

public class LogViewerView : IView
{
    private readonly ILoggerService _logger;

    public string Name => "Log Viewer";

    public LogViewerView(ILoggerService logger)
    {
        _logger = logger;
    }

    public async Task ShowAsync()
    {
        bool stay = true;
        string? currentFilter = null;
        DateTime? dateFilter = null;
        string? searchKeyword = null;

        while (stay)
        {
            ConsoleHelper.ClearScreen();
            ConsoleRenderer.DrawHeader(Messages.LogViewer.Title);

            var logs = await _logger.GetLogsAsync();

            if (currentFilter != null)
                logs = logs.Where(l => l.Level == currentFilter).ToList();
            if (dateFilter.HasValue)
                logs = logs.Where(l => l.Timestamp.Date == dateFilter.Value.Date).ToList();
            if (!string.IsNullOrEmpty(searchKeyword))
                logs = logs.Where(l =>
                    l.Message.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) ||
                    l.Category.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase)).ToList();

            ConsoleHelper.WriteLineColored("");
            ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}Total Logs: {logs.Count}{AppConstants.Colors.Reset}");

            if (currentFilter != null)
                ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Info}Filter: {currentFilter}{AppConstants.Colors.Reset}");
            if (dateFilter.HasValue)
                ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Info}Date: {dateFilter.Value:yyyy-MM-dd}{AppConstants.Colors.Reset}");
            if (!string.IsNullOrEmpty(searchKeyword))
                ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Info}Search: \"{searchKeyword}\"{AppConstants.Colors.Reset}");

            ConsoleHelper.WriteLineColored("");

            if (logs.Count == 0)
            {
                ConsoleHelper.WriteInfo(Messages.LogViewer.NoLogs);
            }
            else
            {
                int displayCount = Math.Min(15, logs.Count);
                ConsoleHelper.WriteTableHeader(
                    new[] { "#", "Timestamp", "Level", "Category", "Message" },
                    new[] { 4, 22, 12, 15, 55 });

                for (int i = 0; i < displayCount; i++)
                {
                    var log = logs[i];
                    string msg = log.Message.Length > 52 ? log.Message[..52] + "..." : log.Message;

                    ConsoleHelper.WriteTableRow(
                        new[] {
                            (i + 1).ToString(),
                            log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                            log.Level,
                            log.Category,
                            msg
                        },
                        new[] { 4, 22, 12, 15, 55 },
                        AppConstants.Colors.Dim);
                }

                if (logs.Count > 15)
                {
                    ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}... and {logs.Count - 15} more entries{AppConstants.Colors.Reset}");
                }
            }

            ConsoleHelper.WriteLineColored("");
            ConsoleRenderer.DrawMenuTitle("Options");
            ConsoleRenderer.DrawMenuItem("F", "Filter by Level", "Filter logs by type (I/W/E/S)");
            ConsoleRenderer.DrawMenuItem("D", "Filter by Date", "Show logs for a specific date");
            ConsoleRenderer.DrawMenuItem("S", "Search Logs", "Search by keyword");
            ConsoleRenderer.DrawMenuItem("E", "Export Logs", "Export to JSON file");
            ConsoleRenderer.DrawMenuItem("C", "Clear Filters", "Remove all filters");
            ConsoleRenderer.DrawMenuItem("R", "Refresh", "Refresh log list");
            ConsoleRenderer.DrawMenuItem("B", "Back to Dashboard", "");

            ConsoleKey key;
            try { key = Console.ReadKey(true).Key; } catch { return; }
            switch (key)
            {
                case ConsoleKey.F:
                    ConsoleHelper.WriteLineColored("");
                    ConsoleHelper.WriteColored($"  {AppConstants.Icons.Question} Filter (I=Info, W=Warning, E=Error, S=Success, A=All): ");
                    string? filter;
                    try { filter = Console.ReadLine()?.Trim().ToUpperInvariant(); } catch { filter = null; }
                    currentFilter = filter switch
                    {
                        "I" => "INFORMATION",
                        "W" => "WARNING",
                        "E" => "ERROR",
                        "S" => "SUCCESS",
                        "A" => null,
                        _ => currentFilter
                    };
                    break;
                case ConsoleKey.D:
                    ConsoleHelper.WriteLineColored("");
                    string dateStr = InputHelper.ReadString("Enter date (yyyy-MM-dd)");
                    if (DateTime.TryParse(dateStr, out var dt))
                        dateFilter = dt;
                    else
                        ConsoleHelper.WriteWarning("Invalid date format.");
                    break;
                case ConsoleKey.S:
                    ConsoleHelper.WriteLineColored("");
                    string keyword = InputHelper.ReadString("Enter search keyword");
                    searchKeyword = keyword;
                    break;
                case ConsoleKey.E:
                    string exportPath = Path.Combine(Directory.GetCurrentDirectory(), $"logs_export_{DateTime.Now:yyyyMMdd_HHmmss}.json");
                    ConsoleHelper.WriteLineColored("");
                    using (var spinner = new Utilities.ConsoleSpinner("Exporting..."))
                    {
                        spinner.Start();
                        await _logger.ExportLogsAsync(exportPath);
                        spinner.Stop();
                    }
                    ConsoleHelper.DisplayNotification(string.Format(Messages.LogViewer.ExportSuccess, exportPath), "success");
                    ConsoleHelper.WaitForKey();
                    break;
                case ConsoleKey.C:
                    currentFilter = null;
                    dateFilter = null;
                    searchKeyword = null;
                    ConsoleHelper.WriteSuccess("Filters cleared.");
                    await Task.Delay(500);
                    break;
                case ConsoleKey.R:
                    break;
                case ConsoleKey.B:
                case ConsoleKey.Escape:
                    stay = false;
                    break;
            }
        }
    }
}
