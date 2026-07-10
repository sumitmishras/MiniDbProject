using System.Text.RegularExpressions;

namespace MiniDbProject.Helpers;

public static class ValidationHelper
{
    public static bool IsValidUsername(string username)
    {
        return !string.IsNullOrWhiteSpace(username) &&
               username.Length >= 3 &&
               username.Length <= 50 &&
               Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$");
    }

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsValidDatabaseName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) &&
               name.Length <= 128 &&
               Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_$#@]*$");
    }

    public static bool IsValidServerName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Length <= 256;
    }

    public static bool IsLocalSqlInstance(string instance)
    {
        if (string.IsNullOrWhiteSpace(instance)) return false;
        string lower = instance.ToLowerInvariant();
        if (lower.Contains("(localdb)") || lower.Contains("localhost") ||
            lower.StartsWith(".") || lower.StartsWith("127.") ||
            lower == "(local)" || lower.StartsWith("local")) return true;
        if (Regex.IsMatch(lower, @"^[a-z0-9-]+\\[a-z0-9-]+$") &&
            !lower.Contains(".")) return true;
        return false;
    }

    public static bool IsValidFilePath(string path)
    {
        return !string.IsNullOrWhiteSpace(path) &&
               File.Exists(path) &&
               path.EndsWith(".sql", StringComparison.OrdinalIgnoreCase);
    }

    public static string? GetValidationError(string field, string value)
    {
        return field.ToLowerInvariant() switch
        {
            "username" => IsValidUsername(value) ? null :
                "Username must be 3-50 characters, letters, numbers, underscores only.",
            "email" => IsValidEmail(value) ? null :
                "Please enter a valid email address (e.g., user@example.com).",
            "databasename" => IsValidDatabaseName(value) ? null :
                "Database name must start with a letter or underscore, max 128 characters.",
            "servername" => IsValidServerName(value) ? null :
                "Server name cannot be empty and max 256 characters.",
            "password" => value.Length >= 8 ? null :
                "Password must be at least 8 characters long.",
            _ => null
        };
    }

    public static bool TryValidate(string field, string value, out string errorMessage)
    {
        errorMessage = GetValidationError(field, value) ?? "";
        return string.IsNullOrEmpty(errorMessage);
    }
}
