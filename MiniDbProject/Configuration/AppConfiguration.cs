using Microsoft.Extensions.Configuration;

namespace MiniDbProject.Configuration;

public class AppConfiguration
{
    public ApplicationSettingsConfig ApplicationSettings { get; set; } = new();
    public SqlServerConfig SqlServer { get; set; } = new();
    public LoggingConfig Logging { get; set; } = new();
    public AuthenticationConfig Authentication { get; set; } = new();
    public DatabaseCreationConfig DatabaseCreation { get; set; } = new();
    public SecurityConfig Security { get; set; } = new();

    public static AppConfiguration Load()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var appConfig = new AppConfiguration();
        config.Bind(appConfig);
        return appConfig;
    }
}

public class ApplicationSettingsConfig
{
    public string ApplicationName { get; set; } = "Mini Database Project";
    public string Version { get; set; } = "1.0.0";
    public string Author { get; set; } = "Unknown";
    public string BuildDate { get; set; } = "";
    public string FrameworkVersion { get; set; } = "NET 10.0";
    public int ConsoleWidth { get; set; } = 120;
    public int ConsoleHeight { get; set; } = 40;
}

public class SqlServerConfig
{
    public string LocalInstance { get; set; } = "(localdb)\\MSSQLLocalDB";
    public int ConnectionTimeoutSeconds { get; set; } = 30;
    public int CommandTimeoutSeconds { get; set; } = 120;
}

public class LoggingConfig
{
    public string LogDirectory { get; set; } = "Logs";
    public string LogFileNamePattern { get; set; } = "app_{yyyyMMdd}.log";
    public int MaxLogFiles { get; set; } = 30;
    public string LogLevel { get; set; } = "Information";
}

public class AuthenticationConfig
{
    public int MaxLoginAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
    public int PasswordMinLength { get; set; } = 8;
    public bool RequireMfaByDefault { get; set; } = false;
    public int SessionTimeoutMinutes { get; set; } = 60;
}

public class DatabaseCreationConfig
{
    public int ProgressUpdateIntervalMs { get; set; } = 500;
    public bool EnablePauseResume { get; set; } = false;
    public bool BackupBeforeCreate { get; set; } = false;
    public int MaxScriptSizeMb { get; set; } = 100;
}

public class SecurityConfig
{
    public int HashIterations { get; set; } = 100000;
    public int HashSaltSize { get; set; } = 16;
    public int HashKeySize { get; set; } = 32;
}
