using System.Text.RegularExpressions;

namespace MiniDbWpf.Helpers;

public static class ValidationHelper
{
    public static bool IsValidEmail(string email)
        => Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);

    public static bool IsValidDatabaseName(string name)
        => Regex.IsMatch(name, @"^[a-zA-Z_][a-zA-Z0-9_]{0,127}$");

    public static bool IsValidServerName(string name)
        => !string.IsNullOrWhiteSpace(name) && name.Length <= 256;

    public static bool IsLocalInstance(string server)
    {
        var s = server.Trim().ToLowerInvariant();
        return s == "." || s == "(local)" || s == "localhost" || s.StartsWith("localhost\\") ||
               s.StartsWith(".\\") || s.StartsWith("(localdb)\\") || s.Equals("localdb") ||
               s.StartsWith("np:") || s.StartsWith("lpc:") ||
               s.Contains("localdb", StringComparison.OrdinalIgnoreCase);
    }
}
