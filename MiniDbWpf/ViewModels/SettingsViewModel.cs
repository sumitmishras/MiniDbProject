using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniDbWpf.Services;

namespace MiniDbWpf.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ILoggerService _logger;

    [ObservableProperty] private string _logLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    [ObservableProperty] private bool _isDarkTheme = true;
    [ObservableProperty] private bool _autoRefreshLogs = true;
    [ObservableProperty] private string _statusText = string.Empty;

    public SettingsViewModel(ILoggerService logger)
    {
        _logger = logger;
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        StatusText = "Settings saved.";
        await _logger.LogSuccess("Settings updated.");
    }

    [RelayCommand]
    private async Task BrowseLogLocation()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Select Folder"
        };
        if (dialog.ShowDialog() == true)
        {
            LogLocation = System.IO.Path.GetDirectoryName(dialog.FileName) ?? LogLocation;
        }
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ResetDefaults()
    {
        IsDarkTheme = true;
        AutoRefreshLogs = true;
        LogLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        StatusText = "Defaults restored.";
        await _logger.LogInfo("Settings reset to defaults.");
    }
}
