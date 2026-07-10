namespace MiniDbProject.Models;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public bool IsMfaEnabled { get; set; }
    public string? MfaSecretKey { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public string? LastLoginIp { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Department { get; set; }
    public string? Role { get; set; } = "User";
}
