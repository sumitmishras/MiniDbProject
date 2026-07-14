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

    [ObservableProperty] private string _sourceServer = "";
    [ObservableProperty] private string _sourceDatabase = "";
    [ObservableProperty] private string _sourceUser = "sa";
    [ObservableProperty] private string _sourcePassword = "";
    [ObservableProperty] private bool _useSqlAuth = true;

    [ObservableProperty] private string _destinationServer = @"(localdb)\MSSQLLocalDB";
    [ObservableProperty] private string _destinationDatabase = "MiniDB";

    [ObservableProperty] private DateTime _fromDate = DateTime.Today.AddDays(-7);
    [ObservableProperty] private DateTime _toDate = DateTime.Today;
    [ObservableProperty] private bool _dryRun = true;

    [ObservableProperty] private string _statusText = "";
    [ObservableProperty] private bool _isValidated;
    [ObservableProperty] private bool _isCreating;
    [ObservableProperty] private string _validationMessage = "";

    public CreateDatabaseViewModel(IDatabaseService database, ILoggerService logger)
    {
        _database = database; _logger = logger;
    }

    [RelayCommand]
    private async Task Validate()
    {
        StatusText = "Validating connections...";
        ValidationMessage = "";
        IsValidated = false;

        if (string.IsNullOrWhiteSpace(SourceServer))
        {
            StatusText = "Source server not set";
            ValidationMessage = "Enter your remote source server (e.g. 10.10.2.12 or server\\instance).";
            return;
        }
        if (string.IsNullOrWhiteSpace(SourceDatabase))
        {
            StatusText = "Source database not set";
            ValidationMessage = "Enter the source database name on that server.";
            return;
        }
        if (UseSqlAuth && string.IsNullOrWhiteSpace(SourcePassword))
        {
            StatusText = "SQL Auth password missing";
            ValidationMessage = "Source password is required when using SQL Auth.";
            return;
        }

        var sourceResult = await _database.ValidateConnectionAsync(
            SourceServer, SourceDatabase,
            UseSqlAuth ? SourceUser : null,
            UseSqlAuth ? SourcePassword : null);

        if (!sourceResult.Success)
        {
            StatusText = "Source connection failed!";
            ValidationMessage = sourceResult.Message;
            return;
        }

        var destResult = await _database.ValidateConnectionAsync(
            DestinationServer, "master", null, null);

        if (!destResult.Success)
        {
            StatusText = "Destination connection failed!";
            ValidationMessage = destResult.Message;
            return;
        }

        IsValidated = true;
        StatusText = "All connections validated.";
        ValidationMessage = "Source and destination OK.";
        await _logger.LogSuccess("Database connections validated.");
    }

    [RelayCommand]
    private async Task CreateDatabase()
    {
        if (!IsValidated) return;
        IsCreating = true;
        StatusText = "Starting MiniDB creation...";

        var dialog = new ProgressDialog(_database, _logger,
            SourceServer, SourceDatabase,
            UseSqlAuth ? SourceUser : null,
            UseSqlAuth ? SourcePassword : null,
            DestinationServer, DestinationDatabase,
            FromDate, ToDate, DryRun);
        dialog.ShowDialog();

        IsCreating = false;
        IsValidated = false;
        StatusText = "MiniDB creation completed.";
    }
}
