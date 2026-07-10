using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniDbWpf.Services;

namespace MiniDbWpf.ViewModels;

public partial class ProgressViewModel : ObservableObject
{
    private readonly IDatabaseService _database;
    private readonly ILoggerService _logger;
    private CancellationTokenSource? _cts;

    [ObservableProperty] private int _percent;
    [ObservableProperty] private string _stage = "Initializing...";
    [ObservableProperty] private string _currentTask = "Starting...";
    [ObservableProperty] private string _elapsed = "00:00:00";
    [ObservableProperty] private string _eta = "00:00:20";
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private bool _isCompleted;
    [ObservableProperty] private bool _isCancelled;
    [ObservableProperty] private string _resultDetails = string.Empty;
    public ObservableCollection<string> Logs { get; } = [];

    public ProgressViewModel(IDatabaseService database, ILoggerService logger)
    {
        _database = database; _logger = logger;
    }

    public async Task StartAsync(string sourceServer, string sourceDatabase,
        string? sourceUser, string? sourcePassword,
        string destinationServer, string destinationDatabase,
        DateTime fromDate, DateTime toDate, bool debug)
    {
        IsRunning = true; IsCompleted = false; IsCancelled = false;
        _cts = new CancellationTokenSource();
        var startTime = DateTime.Now;
        var timer = new System.Timers.Timer(1000);
        timer.Elapsed += (_, _) =>
        {
            var span = DateTime.Now - startTime;
            Elapsed = span.ToString(@"hh\:mm\:ss");
            var remaining = Percent < 95
                ? (int)((100.0 - Percent) / (Percent + 1) * span.TotalSeconds)
                : 0;
            Eta = TimeSpan.FromSeconds(remaining).ToString(@"hh\:mm\:ss");
        };

        try
        {
            timer.Start();
            var progress = new Progress<(int Percent, string Stage, string Task, string Detail)>(p =>
            {
                Percent = p.Percent;
                Stage = p.Stage;
                CurrentTask = p.Task;
                var ts = DateTime.Now.ToString("HH:mm:ss");
                Logs.Insert(0, $"[{ts}] {p.Stage}: {p.Task}");
                if (!string.IsNullOrEmpty(p.Detail))
                    ResultDetails = p.Detail;
            });

            await _database.CreateDatabaseAsync(
                sourceServer, sourceDatabase, sourceUser, sourcePassword,
                destinationServer, destinationDatabase,
                fromDate, toDate, debug,
                progress, _cts.Token);
            IsCompleted = true;
            await _logger.LogSuccess($"Database '{destinationDatabase}' created on {destinationServer}.");
        }
        catch (OperationCanceledException)
        {
            IsCancelled = true;
            Logs.Insert(0, $"[{DateTime.Now:HH:mm:ss}] Cancelled by user.");
            await _logger.LogWarning("Database creation cancelled.");
        }
        catch (Exception ex)
        {
            Logs.Insert(0, $"[{DateTime.Now:HH:mm:ss}] Error: {ex.Message}");
            await _logger.LogError($"Database creation failed: {ex.Message}");
        }
        finally
        {
            timer.Stop();
            timer.Dispose();
            IsRunning = false;
        }
    }

    [RelayCommand]
    private void Cancel() => _cts?.Cancel();
}
