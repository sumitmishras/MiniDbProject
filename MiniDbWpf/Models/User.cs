namespace MiniDbWpf.Models;

public class User
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string MfaSecret { get; set; } = string.Empty;
    public bool IsMfaEnabled { get; set; }
    public DateTime LastLoginDate { get; set; }
}
