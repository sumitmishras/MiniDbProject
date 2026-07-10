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

    [ObservableProperty] private string _destinationServer = ".\\LOCALDB";
    [ObservableProperty] private string _destinationDatabase = "MiniDB";

    [ObservableProperty] private DateTime _fromDate = DateTime.Today.AddDays(-7);
    [ObservableProperty] private DateTime _toDate = DateTime.Today;
    [ObservableProperty] private bool _isDebug = true;

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
            FromDate, ToDate, IsDebug);
        dialog.ShowDialog();

        IsCreating = false;
        IsValidated = false;
        StatusText = "MiniDB creation completed.";
    }
}
