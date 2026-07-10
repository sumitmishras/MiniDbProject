using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniDbWpf.Models;
using MiniDbWpf.Services;

namespace MiniDbWpf.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly IAuthenticationService _auth;
    private readonly ILoggerService _logger;

    [ObservableProperty]
    private string _welcomeMessage = string.Empty;

    public ObservableCollection<StatCard> StatCards { get; } = [];
    public ObservableCollection<string> RecentActivities { get; } = [];

    public DashboardViewModel(IAuthenticationService auth, ILoggerService logger)
    {
        _auth = auth; _logger = logger;
        var user = auth.CurrentUser;
        WelcomeMessage = user != null
            ? $"Welcome back, {user.FullName}!  |  Last login: {user.LastLoginDate:yyyy-MM-dd HH:mm}"
            : "Welcome!";

        StatCards =
        [
            new() { Title = "Users", Value = "12", IconKind = "AccountGroup", Description = "Active users",
                    IconBrush = new SolidColorBrush(Color.FromRgb(88, 88, 232)) },
            new() { Title = "Databases", Value = "42", IconKind = "Database", Description = "Total created",
                    IconBrush = new SolidColorBrush(Color.FromRgb(0, 200, 83)) },
            new() { Title = "Logs", Value = "845", IconKind = "FileDocument", Description = "System events",
                    IconBrush = new SolidColorBrush(Color.FromRgb(255, 109, 0)) },
            new() { Title = "System", Value = "Healthy", IconKind = "ShieldCheck", Description = "All systems go",
                    IconBrush = new SolidColorBrush(Color.FromRgb(0, 188, 212)) }
        ];

        RecentActivities =
        [
            "✔ Login Successful",
            "✔ Database 'HRMS_Dev' Created",
            "✔ Log Generated - Daily Report",
            "✔ User Profile Updated",
            "✔ System Health Check Passed"
        ];

        _ = logger.LogInfo("Dashboard page loaded.");
    }

    [RelayCommand]
    private void Refresh()
    {
        var user = _auth.CurrentUser;
        WelcomeMessage = user != null
            ? $"Welcome back, {user.FullName}!  |  Last login: {user.LastLoginDate:yyyy-MM-dd HH:mm}"
            : "Welcome!";
    }
}
