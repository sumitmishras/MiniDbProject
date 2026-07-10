using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using MiniDbWpf.Models;
using MiniDbWpf.Services;
using MiniDbWpf.Views;

namespace MiniDbWpf.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAuthenticationService _auth;
    private readonly ILoggerService _logger;
    private readonly IProfileService _profile;
    private readonly IDatabaseService _database;

    [ObservableProperty]
    private object? _currentPage;

    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private bool _isSidebarCollapsed;

    [ObservableProperty]
    private bool _isDarkTheme = true;

    public ObservableCollection<NavigationItem> NavigationItems { get; } = [];

    public MainViewModel(IAuthenticationService auth, ILoggerService logger, IProfileService profile, IDatabaseService database)
    {
        _auth = auth; _logger = logger; _profile = profile; _database = database;
        CurrentUser = auth.CurrentUser;

        NavigationItems =
        [
            new() { Title = "Dashboard", IconKind = "ViewDashboard", PageType = typeof(DashboardViewModel) },
            new() { Title = "Create Database", IconKind = "DatabasePlus", PageType = typeof(CreateDatabaseViewModel) },
            new() { Title = "Profile", IconKind = "AccountCircle", PageType = typeof(ProfileViewModel) },
            new() { Title = "Logs", IconKind = "FileDocument", PageType = typeof(LogsViewModel) },
            new() { Title = "Settings", IconKind = "Cog", PageType = typeof(SettingsViewModel) },
            new() { Title = "Help", IconKind = "HelpCircle", PageType = typeof(HelpViewModel) },
            new() { Title = "About", IconKind = "Information", PageType = typeof(AboutViewModel) }
        ];

        NavigateToDashboard();
    }

    private void NavigateToDashboard()
    {
        var type = typeof(DashboardViewModel);
        foreach (var item in NavigationItems)
            item.IsSelected = item.PageType == type;

        CurrentPage = new DashboardViewModel(_auth, _logger);
    }

    [RelayCommand]
    private void NavigateToType(string pageTypeName)
    {
        var item = NavigationItems.FirstOrDefault(i => i.PageType.Name == pageTypeName);
        if (item == null) return;
        foreach (var ni in NavigationItems) ni.IsSelected = ni == item;

        CurrentPage = item.PageType.Name switch
        {
            nameof(DashboardViewModel) => new DashboardViewModel(_auth, _logger),
            nameof(CreateDatabaseViewModel) => new CreateDatabaseViewModel(_database, _logger),
            nameof(ProfileViewModel) => new ProfileViewModel(_profile, _auth, _logger),
            nameof(LogsViewModel) => new LogsViewModel(_logger),
            nameof(SettingsViewModel) => new SettingsViewModel(_logger),
            nameof(HelpViewModel) => new HelpViewModel(),
            nameof(AboutViewModel) => new AboutViewModel(),
            _ => CurrentPage
        };
    }

    [RelayCommand]
    private void ToggleSidebar()
    {
        IsSidebarCollapsed = !IsSidebarCollapsed;
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        IsDarkTheme = !IsDarkTheme;
        var paletteHelper = new PaletteHelper();
        var theme = paletteHelper.GetTheme();
        theme.SetBaseTheme(IsDarkTheme ? BaseTheme.Dark : BaseTheme.Light);
        paletteHelper.SetTheme(theme);
    }

    [RelayCommand]
    private async Task Logout()
    {
        await _logger.LogInfo($"User '{CurrentUser?.Username}' logged out.");
        await _auth.LogoutAsync();
        System.Windows.Application.Current.Shutdown();
    }
}
