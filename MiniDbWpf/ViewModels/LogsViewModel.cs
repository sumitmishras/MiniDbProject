using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniDbWpf.Models;
using MiniDbWpf.Services;

namespace MiniDbWpf.ViewModels;

public partial class LogsViewModel : ObservableObject
{
    private readonly ILoggerService _logger;
    private List<LogEntry> _allLogs = [];

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _selectedLevel = "All";

    public ObservableCollection<LogEntry> Logs { get; } = [];
    public List<string> Levels { get; } = ["All", "Info", "Success", "Warning", "Error"];

    public LogsViewModel(ILoggerService logger)
    {
        _logger = logger;
        _ = LoadLogs();
    }

    partial void OnSelectedLevelChanged(string value) => ApplyFilters();
    partial void OnSearchTextChanged(string value) => ApplyFilters();

    public async Task LoadLogs()
    {
        _allLogs = await _logger.GetLogsAsync();
        ApplyFilters();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        await LoadLogs();
    }

    private void ApplyFilters()
    {
        var filtered = _allLogs.AsEnumerable();

        if (SelectedLevel != "All")
            filtered = filtered.Where(l => l.Level == SelectedLevel);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.ToLower();
            filtered = filtered.Where(l =>
                l.Message.ToLower().Contains(s) ||
                l.Category.ToLower().Contains(s));
        }

        Logs.Clear();
        foreach (var log in filtered)
            Logs.Add(log);
    }

    [RelayCommand]
    private async Task Export()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JSON Files (*.json)|*.json",
            FileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.json"
        };
        if (dialog.ShowDialog() != true) return;

        var logs = Logs.ToList();
        var json = System.Text.Json.JsonSerializer.Serialize(logs, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(dialog.FileName, json);
    }
}
