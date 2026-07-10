using CommunityToolkit.Mvvm.ComponentModel;

namespace MiniDbWpf.ViewModels;

public partial class HelpViewModel : ObservableObject
{
    public string HelpText { get; } = """
        Mini Database Creator v2.0

        How to use:

        1. DASHBOARD
           View system statistics and recent activities.

        2. CREATE DATABASE
           - Enter source server and database
           - Select SQL script file
           - Click Validate to verify connections
           - Click Create Database to start the process

        3. PROFILE
           Update your profile information, change password,
           and manage MFA settings.

        4. LOGS
           View all system logs. Use search and filters to
           find specific entries. Export logs as JSON.

        5. SETTINGS
           Configure application preferences.

        SUPPORT
        For help, contact support@minidb.local
        """;
}
