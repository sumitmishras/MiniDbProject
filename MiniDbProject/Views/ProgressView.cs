using MiniDbProject.Constants;
using MiniDbProject.Database;
using MiniDbProject.DTOs;
using MiniDbProject.Helpers;
using MiniDbProject.Logging;
using MiniDbProject.Models;
using MiniDbProject.Resources;
using MiniDbProject.Utilities;

namespace MiniDbProject.Views;

public class ProgressView : IView
{
    private readonly IDatabaseService _dbService;
    private readonly ILoggerService _logger;
    private CancellationTokenSource? _cts;

    public string Name => "Progress";

    public ProgressView(IDatabaseService dbService, ILoggerService logger)
    {
        _dbService = dbService;
        _logger = logger;
    }

    public async Task ShowAsync()
    {
        return;
    }

    public async Task ShowAsync(WizardData wizardData)
    {
        _cts = new CancellationTokenSource();

        var selection = new DatabaseSelectionDto
        {
            PortalName = wizardData.SelectedPortalName ?? "",
            ServerName = wizardData.SelectedServerName ?? "",
            SourceDatabaseName = wizardData.SourceDatabaseName ?? "",
            DestinationInstance = wizardData.DestinationInstance ?? "(localdb)\\MSSQLLocalDB",
            DestinationDatabaseName = wizardData.DestinationDatabaseName ?? "",
            ScriptFilePath = wizardData.ScriptFilePath,
            IsLocalDestination = true
        };

        await _logger.LogInfo($"Database creation started: {selection.SourceDatabaseName} -> {selection.DestinationDatabaseName}");

        var progressTask = _dbService.CreateDatabaseAsync(selection, _cts.Token);
        var progressDisplay = DisplayProgressAsync(progressTask, selection);

        try
        {
            var result = await progressTask;
            await progressDisplay;

            ConsoleHelper.ClearScreen();
            ConsoleRenderer.DrawHeader("Database Creation Complete");

            if (result.IsCompleted)
            {
                ConsoleHelper.DisplayNotification(string.Format(Messages.DatabaseWizard.Completed), "success");
            }
            else if (result.IsCancelled)
            {
                ConsoleHelper.DisplayNotification(Messages.DatabaseWizard.Cancelled, "warning");
            }

            ConsoleHelper.WriteLineColored("");
            var finalInfo = new Dictionary<string, string>
            {
                { "Source Database", selection.SourceDatabaseName },
                { "Destination", $"{selection.DestinationInstance}\\{selection.DestinationDatabaseName}" },
                { "Steps Completed", $"{result.CompletedSteps} of {result.TotalSteps}" },
                { "Total Time", TimeHelper.FormatTimeSpan(result.Elapsed) },
                { "Records Processed", $"{result.RecordsProcessed:N0}" },
                { "Status", result.StatusMessage }
            };
            ConsoleRenderer.DrawInfoPanel("Final Status", finalInfo);
        }
        catch (OperationCanceledException)
        {
            ConsoleHelper.ClearScreen();
            ConsoleRenderer.DrawHeader("Database Creation Cancelled");
            ConsoleHelper.DisplayNotification(Messages.DatabaseWizard.Cancelled, "warning");
            await _logger.LogWarning($"Database creation cancelled by user: {selection.SourceDatabaseName}");
        }
        catch (Exception ex)
        {
            ConsoleHelper.ClearScreen();
            ConsoleRenderer.DrawHeader("Database Creation Failed");
            ConsoleHelper.DisplayNotification($"Error: {ex.Message}", "error");
            await _logger.LogError($"Database creation failed: {ex.Message}", ex);
        }
        finally
        {
            _cts?.Dispose();
        }

        ConsoleHelper.WaitForKey();
    }

    private async Task DisplayProgressAsync(Task<ProgressInfo> progressTask, DatabaseSelectionDto selection)
    {
        var spinner = new ConsoleSpinner("Processing...");
        var startTime = DateTime.Now;

        while (!progressTask.IsCompleted && !progressTask.IsFaulted && !progressTask.IsCanceled)
        {
            bool keyPressed = false;
            ConsoleKeyInfo keyInfo = default;
            try
            {
                keyPressed = Console.KeyAvailable;
                if (keyPressed) keyInfo = Console.ReadKey(true);
            }
            catch { }

            if (keyPressed)
            {
                bool cancelRequested = false;
                if (keyInfo.Key == ConsoleKey.C && (keyInfo.Modifiers & ConsoleModifiers.Control) != 0)
                    cancelRequested = true;
                if (keyInfo.Key == ConsoleKey.Escape)
                    cancelRequested = true;

                if (cancelRequested)
                {
                    if (ConsoleHelper.PromptConfirmation(Messages.Common.CancelConfirm))
                    {
                        _cts?.Cancel();
                        await _logger.LogInfo("User requested cancellation of database creation.");
                        spinner.Stop();
                        return;
                    }
                }
            }

            await RenderProgressScreen(selection, startTime, 0, "Initializing...", "", "", 0, "", DateTime.Now, false);
            await Task.Delay(200);
        }

        spinner.Stop();

        if (progressTask.IsCompletedSuccessfully)
        {
            var result = progressTask.Result;
            await RenderProgressScreen(selection, startTime,
                result.Percentage, result.StatusMessage,
                result.CurrentStage, result.CurrentTask,
                result.CompletedSteps, result.CurrentScript,
                result.StartTime, true);
        }
    }

    private async Task RenderProgressScreen(DatabaseSelectionDto selection, DateTime startTime,
        int percentage, string status, string stage, string task,
        int completedSteps, string currentScript, DateTime taskStartTime, bool final)
    {
        ConsoleHelper.ClearScreen();
        ConsoleRenderer.DrawHeader("Database Creation Progress");

        ConsoleHelper.WriteLineColored("");
        ConsoleHelper.WriteProgressBar(percentage);
        ConsoleHelper.WriteLineColored("\n");

        var progressInfo = new Dictionary<string, string>
        {
            { "Source", $"{selection.ServerName}\\{selection.SourceDatabaseName}" },
            { "Destination", $"{selection.DestinationInstance}\\{selection.DestinationDatabaseName}" },
            { "Status", status },
            { "Stage", stage },
            { "Current Task", task },
            { "Completed Steps", completedSteps.ToString() },
            { "Total Steps", _dbService.TotalSteps.ToString() },
            { "Elapsed Time", TimeHelper.FormatTimeSpan(DateTime.Now - startTime) }
        };

        if (percentage > 0)
        {
            var totalEst = (DateTime.Now - startTime).TotalMilliseconds / percentage * 100;
            var remaining = totalEst - (DateTime.Now - startTime).TotalMilliseconds;
            progressInfo["ETA"] = TimeHelper.FormatTimeSpan(TimeSpan.FromMilliseconds(Math.Max(0, remaining)));
        }
        else
        {
            progressInfo["ETA"] = "Calculating...";
        }

        ConsoleRenderer.DrawInfoPanel("Progress Details", progressInfo);

        if (!string.IsNullOrEmpty(currentScript))
        {
            ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}Executing: {currentScript}{AppConstants.Colors.Reset}");
        }

        if (!final)
        {
            ConsoleHelper.WriteLineColored("");
            ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}Press {AppConstants.Colors.Warning}ESC{AppConstants.Colors.Dim} or {AppConstants.Colors.Warning}Ctrl+C{AppConstants.Colors.Dim} to cancel{AppConstants.Colors.Reset}");
        }

        await Task.Delay(500);
    }
}
