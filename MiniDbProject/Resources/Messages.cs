namespace MiniDbProject.Resources;

public static class Messages
{
    public static class Login
    {
        public const string Title = "User Login";
        public const string UsernamePrompt = "Enter Username";
        public const string PasswordPrompt = "Enter Password";
        public const string MfaPrompt = "Enter MFA Code";
        public const string Success = "Login successful! Welcome back, {0}!";
        public const string Failed = "Login failed. Invalid username or password.";
        public const string LockedOut = "Account locked out. Please try again after {0} minutes.";
        public const string MfaRequired = "Multi-Factor Authentication required.";
        public const string MfaSuccess = "MFA verification successful.";
        public const string MfaFailed = "Invalid MFA code. Please try again.";
        public const string LogoutSuccess = "You have been logged out successfully.";
        public const string SessionExpired = "Your session has expired. Please login again.";
    }

    public static class Dashboard
    {
        public const string Title = "Main Dashboard";
        public const string Welcome = "Welcome, {0}!";
        public const string LastLogin = "Last Login: {0}";
        public const string MenuProfile = "My Profile";
        public const string MenuCreateDb = "Create Database from Script";
        public const string MenuViewLogs = "View Logs";
        public const string MenuHelp = "Help & Support";
        public const string MenuAbout = "About";
        public const string MenuLogout = "Logout";
    }

    public static class Profile
    {
        public const string Title = "My Profile";
        public const string UpdateSuccess = "Profile updated successfully.";
        public const string PasswordChangeSuccess = "Password changed successfully.";
        public const string PasswordChangeFailed = "Current password is incorrect.";
        public const string MfaEnabled = "MFA has been enabled successfully.";
        public const string MfaDisabled = "MFA has been disabled successfully.";
        public const string MfaCode = "Your MFA secret key is: {0}";
    }

    public static class DatabaseWizard
    {
        public const string Title = "Database Creation Wizard";
        public const string StepSource = "Source Selection";
        public const string StepDest = "Destination Configuration";
        public const string StepValidation = "Validation";
        public const string StepSummary = "Summary & Confirmation";
        public const string StepProgress = "Creating Database...";
        public const string SelectPortal = "Select Source Portal";
        public const string SelectServer = "Select Server";
        public const string SelectDatabase = "Select Source Database";
        public const string DestLocalOnly = "Destination must be a local SQL Server instance.";
        public const string ConfirmStart = "Ready to start database creation?";
        public const string Completed = "Database creation completed successfully!";
        public const string Cancelled = "Database creation was cancelled.";
    }

    public static class LogViewer
    {
        public const string Title = "Log Viewer";
        public const string NoLogs = "No log entries found.";
        public const string ExportSuccess = "Logs exported to: {0}";
        public const string FilterPrompt = "Filter by level (I/W/E/S all): ";
    }

    public static class Help
    {
        public const string Title = "Help & Support";
        public const string About = "About";
    }

    public static class Errors
    {
        public const string Unexpected = "An unexpected error occurred: {0}";
        public const string Network = "Network error. Please check your connection.";
        public const string ServerUnreachable = "Server '{0}' is not reachable.";
        public const string DatabaseNotFound = "Database '{0}' not found on server.";
        public const string InvalidInput = "Invalid input. Please try again.";
        public const string DuplicateDb = "Database '{0}' already exists on the destination.";
        public const string ScriptNotFound = "SQL script file not found: {0}";
        public const string PermissionDenied = "Permission denied. Contact your administrator.";
        public const string Timeout = "Operation timed out after {0} seconds.";
    }

    public static class Validation
    {
        public const string Required = "{0} is required.";
        public const string MaxLength = "{0} cannot exceed {1} characters.";
        public const string MinLength = "{0} must be at least {1} characters.";
        public const string InvalidFormat = "Invalid format for {0}.";
        public const string NoRemoteDest = "Remote destination servers are not allowed. Only local instances are permitted.";
    }

    public static class Common
    {
        public const string PressAnyKey = "Press any key to continue...";
        public const string Processing = "Processing...";
        public const string Loading = "Loading...";
        public const string Confirmation = "Are you sure?";
        public const string CancelConfirm = "Are you sure you want to cancel?";
        public const string ExitConfirm = "Are you sure you want to exit?";
        public const string Yes = "Yes";
        public const string No = "No";
        public const string Back = "Go Back";
        public const string Exit = "Exit";
        public const string Save = "Save";
        public const string Cancel = "Cancel";
        public const string Retry = "Retry";
    }
}
