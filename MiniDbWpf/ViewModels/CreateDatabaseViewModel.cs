using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniDbWpf.Services;
using MiniDbWpf.Views;

namespace MiniDbWpf.ViewModels;

public partial class CreateDatabaseViewModel : ObservableObject
{
    private readonly IDatabaseService _database;
    private readonly ILoggerService _logger;

    [ObservableProperty] private string _sourceServer = "DEV_SERVER_01";
    [ObservableProperty] private string _sourceDatabase = "HRMS";
    [ObservableProperty] private string _destinationServer = "LocalDB (Fixed)";
    [ObservableProperty] private string _scriptPath = string.Empty;
    [ObservableProperty] private string _statusText = string.Empty;
    [ObservableProperty] private bool _isValidated;
    [ObservableProperty] private bool _isCreating;

    public CreateDatabaseViewModel(IDatabaseService database, ILoggerService logger)
    {
        _database = database; _logger = logger;
    }

    [RelayCommand]
    private async Task BrowseScript()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "SQL Files (*.sql)|*.sql|All Files (*.*)|*.*",
            Title = "Select SQL Script"
        };
        if (dialog.ShowDialog() == true)
            ScriptPath = dialog.FileName;
    }

    [RelayCommand]
    private async Task Validate()
    {
        StatusText = "Validating...";
        var connOk = await _database.ValidateConnectionAsync(SourceServer, SourceDatabase);
        if (!connOk)
        {
            StatusText = "Connection validation failed!";
            return;
        }

        if (!string.IsNullOrEmpty(ScriptPath))
        {
            var scriptOk = await _database.ValidateScriptAsync(ScriptPath);
            if (!scriptOk)
            {
                StatusText = "Script validation failed!";
                return;
            }
        }

        IsValidated = true;
        StatusText = "Validation passed. Ready to create.";
        await _logger.LogSuccess("Database validation passed.");
    }

    [RelayCommand]
    private async Task CreateDatabase()
    {
        if (!IsValidated) return;
        IsCreating = true;
        StatusText = "Creating database...";

        var dialog = new ProgressDialog(_database, _logger, SourceServer, SourceDatabase, ScriptPath);
        dialog.ShowDialog();

        IsCreating = false;
        IsValidated = false;
        StatusText = "Database creation completed.";
    }
}
