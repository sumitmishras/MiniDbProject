using MiniDbProject.Constants;
using MiniDbProject.Database;
using MiniDbProject.DTOs;
using MiniDbProject.Helpers;
using MiniDbProject.Logging;
using MiniDbProject.Models;
using MiniDbProject.Resources;

namespace MiniDbProject.Views;

public class DatabaseWizardView : IView
{
    private readonly IDatabaseService _dbService;
    private readonly ILoggerService _logger;
    private readonly ProgressView _progressView;

    public string Name => "Database Wizard";

    private readonly List<WizardStep> _steps = new()
    {
        new WizardStep { StepNumber = 1, Title = "Source Selection", Description = "Select source portal and database" },
        new WizardStep { StepNumber = 2, Title = "Destination Configuration", Description = "Configure local destination" },
        new WizardStep { StepNumber = 3, Title = "Validation", Description = "Validate source, destination, and script" },
        new WizardStep { StepNumber = 4, Title = "Summary & Confirmation", Description = "Review and confirm your selections" }
    };

    public DatabaseWizardView(IDatabaseService dbService, ILoggerService logger, ProgressView progressView)
    {
        _dbService = dbService;
        _logger = logger;
        _progressView = progressView;
    }

    public async Task ShowAsync()
    {
        var wizardData = new WizardData();

        foreach (var step in _steps)
        {
            step.IsActive = true;
            bool continueWizard = await ShowStep(step, wizardData);
            step.IsActive = false;

            if (!continueWizard) return;

            step.IsCompleted = true;
        }

        if (wizardData.Confirmed)
        {
            await _progressView.ShowAsync(wizardData);
        }
    }

    private async Task<bool> ShowStep(WizardStep step, WizardData data)
    {
        return step.StepNumber switch
        {
            1 => await ShowSourceSelectionStep(data),
            2 => await ShowDestinationStep(data),
            3 => await ShowValidationStep(data),
            4 => await ShowSummaryStep(data),
            _ => true
        };
    }

    private async Task<bool> ShowSourceSelectionStep(WizardData data)
    {
        ConsoleHelper.ClearScreen();
        ConsoleRenderer.DrawHeader(Messages.DatabaseWizard.Title);
        DrawWizardProgress(1);
        ConsoleRenderer.DrawHeader($"Step 1: {Messages.DatabaseWizard.StepSource}");

        ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}Select a portal, server, and source database.{AppConstants.Colors.Reset}");
        ConsoleHelper.WriteLineColored("");

        using (var spinner = new Utilities.ConsoleSpinner("Loading available portals..."))
        {
            spinner.Start();
            var portals = await _dbService.GetAvailablePortalsAsync();
            spinner.Stop();
        }

        var portals2 = await _dbService.GetAvailablePortalsAsync();

        if (portals2.Count == 0)
        {
            ConsoleHelper.WriteError("No portals available. Please check your network connection.");
            ConsoleHelper.WaitForKey();
            return false;
        }

        var menu = new MenuSystem("Select Source Portal");
        foreach (var portal in portals2)
        {
            menu.AddItem(portal.PortalId.ToString(), $"{portal.PortalName} ({portal.ServerName})",
                $"{portal.Description} - {portal.Databases.Count} databases", icon: AppConstants.Icons.Server);
        }
        menu.AddSeparator();
        menu.AddItem("back", "Go Back", icon: AppConstants.Icons.Back);

        string? selected = await menu.ShowAsync();
        if (selected == null || selected == "back") return false;

        if (!int.TryParse(selected, out int portalId)) return false;
        var selectedPortal = portals2.First(p => p.PortalId == portalId);

        var dbMenu = new MenuSystem("Select Source Database");
        foreach (var db in selectedPortal.Databases)
        {
            dbMenu.AddItem(db.DatabaseName, $"{db.DatabaseName}",
                $"{db.EstimatedSizeDisplay} | {db.TableCount} tables", icon: AppConstants.Icons.Database);
        }
        dbMenu.AddSeparator();
        dbMenu.AddItem("back", "Go Back", icon: AppConstants.Icons.Back);

        string? dbSelected = await dbMenu.ShowAsync();
        if (dbSelected == null || dbSelected == "back") return false;

        data.SelectedPortalName = selectedPortal.PortalName;
        data.SelectedServerName = selectedPortal.ServerName;
        data.SourceDatabaseName = dbSelected;
        data.ScriptFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Scripts", $"{dbSelected}.sql");

        ConsoleHelper.DisplayNotification(
            $"Selected: {data.SelectedPortalName} / {data.SelectedServerName} / {data.SourceDatabaseName}", "success");
        ConsoleHelper.WaitForKey();
        return true;
    }

    private async Task<bool> ShowDestinationStep(WizardData data)
    {
        ConsoleHelper.ClearScreen();
        ConsoleRenderer.DrawHeader(Messages.DatabaseWizard.Title);
        DrawWizardProgress(2);
        ConsoleRenderer.DrawHeader($"Step 2: {Messages.DatabaseWizard.StepDest}");

        ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Bold}{AppConstants.Colors.Warning}IMPORTANT: Only local SQL Server instances are allowed.{AppConstants.Colors.Reset}");
        ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}Remote servers are strictly prohibited.{AppConstants.Colors.Reset}");
        ConsoleHelper.WriteLineColored("");

        string defaultInstance = _dbService.GetLocalInstanceName();
        string instance = InputHelper.ReadString(
            $"Local SQL Server Instance [{defaultInstance}]",
            required: true,
            defaultValue: defaultInstance);

        bool isValidLocal = ValidationHelper.IsLocalSqlInstance(instance);
        if (!isValidLocal)
        {
            ConsoleHelper.DisplayNotification(Messages.Validation.NoRemoteDest, "error");
            ConsoleHelper.WriteLineColored("");
            ConsoleHelper.WriteInfo($"Please use a local instance like '{defaultInstance}'.");
            ConsoleHelper.WaitForKey();
            return false;
        }

        data.DestinationInstance = instance;

        string defaultDbName = $"{data.SourceDatabaseName}_Local";
        string dbName = InputHelper.ReadString(
            $"Destination Database Name [{defaultDbName}]",
            required: true,
            defaultValue: defaultDbName);

        data.DestinationDatabaseName = dbName;
        data.IsValidated = true;

        ConsoleHelper.DisplayNotification(
            $"Destination: {data.DestinationInstance} / {data.DestinationDatabaseName}", "success");
        ConsoleHelper.WaitForKey();
        return true;
    }

    private async Task<bool> ShowValidationStep(WizardData data)
    {
        ConsoleHelper.ClearScreen();
        ConsoleRenderer.DrawHeader(Messages.DatabaseWizard.Title);
        DrawWizardProgress(3);
        ConsoleRenderer.DrawHeader($"Step 3: {Messages.DatabaseWizard.StepValidation}");

        ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}Validating your selections...{AppConstants.Colors.Reset}");
        ConsoleHelper.WriteLineColored("");

        bool sourceValid, destValid, scriptValid;
        long estimatedSize;

        using (var spinner = new Utilities.ConsoleSpinner("Validating source database..."))
        {
            spinner.Start();
            sourceValid = await _dbService.ValidateSourceDatabaseAsync(data.SelectedServerName!, data.SourceDatabaseName!);
            spinner.Stop();
        }
        ConsoleHelper.WriteInfo($"Source Database: {(sourceValid ? $"{AppConstants.Colors.Success}Valid{AppConstants.Colors.Reset}" : $"{AppConstants.Colors.Error}Invalid{AppConstants.Colors.Reset}")}");

        using (var spinner = new Utilities.ConsoleSpinner("Validating destination..."))
        {
            spinner.Start();
            destValid = await _dbService.ValidateDestinationAsync(data.DestinationInstance!);
            spinner.Stop();
        }
        ConsoleHelper.WriteInfo($"Destination Instance: {(destValid ? $"{AppConstants.Colors.Success}Valid (Local){AppConstants.Colors.Reset}" : $"{AppConstants.Colors.Error}Invalid or Remote{AppConstants.Colors.Reset}")}");

        using (var spinner = new Utilities.ConsoleSpinner("Validating script file..."))
        {
            spinner.Start();
            scriptValid = await _dbService.ValidateScriptFileAsync(data.ScriptFilePath!);
            spinner.Stop();
        }
        ConsoleHelper.WriteInfo($"SQL Script: {(scriptValid ? $"{AppConstants.Colors.Success}Found{AppConstants.Colors.Reset}" : $"{AppConstants.Colors.Warning}Not Found (will use direct source){AppConstants.Colors.Reset}")}");

        using (var spinner = new Utilities.ConsoleSpinner("Estimating database size..."))
        {
            spinner.Start();
            estimatedSize = await _dbService.EstimateDatabaseSizeAsync(data.ScriptFilePath!);
            spinner.Stop();
        }
        data.EstimatedSizeBytes = estimatedSize;
        data.Dependencies = await _dbService.GetDependenciesAsync(data.ScriptFilePath!);
        string sizeDisplay = estimatedSize switch
        {
            >= 1_000_000_000 => $"{(estimatedSize / 1_000_000_000.0):F2} GB",
            >= 1_000_000 => $"{(estimatedSize / 1_000_000.0):F2} MB",
            >= 1_000 => $"{(estimatedSize / 1_000.0):F2} KB",
            _ => $"{estimatedSize} B"
        };
        ConsoleHelper.WriteInfo($"Estimated Size: {sizeDisplay}");

        if (!sourceValid || !destValid)
        {
            ConsoleHelper.DisplayNotification("Validation failed. Please review your selections.", "error");
            ConsoleHelper.WaitForKey();
            return false;
        }

        ConsoleHelper.DisplayNotification("All validations passed!", "success");
        ConsoleHelper.WaitForKey();
        return true;
    }

    private async Task<bool> ShowSummaryStep(WizardData data)
    {
        ConsoleHelper.ClearScreen();
        ConsoleRenderer.DrawHeader(Messages.DatabaseWizard.Title);
        DrawWizardProgress(4);
        ConsoleRenderer.DrawHeader($"Step 4: {Messages.DatabaseWizard.StepSummary}");

        ConsoleHelper.WriteLineColored("");

        var summary = new Dictionary<string, string>
        {
            { "Source Portal", data.SelectedPortalName ?? "" },
            { "Source Server", data.SelectedServerName ?? "" },
            { "Source Database", data.SourceDatabaseName ?? "" },
            { "Destination Instance", data.DestinationInstance ?? "" },
            { "Destination Database", data.DestinationDatabaseName ?? "" },
            { "Estimated Size", $"{(data.EstimatedSizeBytes / (1024.0 * 1024.0)):F2} MB" },
            { "Script File", data.ScriptFilePath ?? "N/A" }
        };
        ConsoleRenderer.DrawInfoPanel("Summary of Selections", summary);

        if (data.Dependencies.Count > 0)
        {
            ConsoleHelper.WriteLineColored("");
            ConsoleRenderer.DrawMenuTitle("Required Dependencies");
            foreach (var dep in data.Dependencies)
            {
                ConsoleHelper.WriteLineColored($"    {AppConstants.Colors.Dim}*{AppConstants.Colors.Reset} {dep}");
            }
        }

        ConsoleHelper.WriteLineColored("");

        if (ConsoleHelper.PromptConfirmation(Messages.DatabaseWizard.ConfirmStart))
        {
            data.Confirmed = true;
            return true;
        }

        ConsoleHelper.WriteInfo("Wizard cancelled. No changes were made.");
        ConsoleHelper.WaitForKey();
        return false;
    }

    private void DrawWizardProgress(int currentStep)
    {
        ConsoleHelper.WriteLineColored("");
        ConsoleHelper.WriteColored("  ");
        for (int i = 0; i < _steps.Count; i++)
        {
            bool isActive = _steps[i].StepNumber == currentStep;
            bool isCompleted = _steps[i].StepNumber < currentStep;

            string color = isActive ? AppConstants.Colors.Highlight :
                           isCompleted ? AppConstants.Colors.Success :
                           AppConstants.Colors.Dim;

            string status = isCompleted ? "\u2713" : _steps[i].StepNumber.ToString();

            ConsoleHelper.WriteColored($"{color}[{status}]{AppConstants.Colors.Reset} {_steps[i].Title}");
            if (i < _steps.Count - 1)
                ConsoleHelper.WriteColored($" {AppConstants.Colors.Dim}\u2192{AppConstants.Colors.Reset} ");
        }
        ConsoleHelper.WriteLineColored("\n");
    }
}
