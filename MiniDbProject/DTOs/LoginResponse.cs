namespace MiniDbProject.DTOs;

public class LoginResponse
{
    public bool IsSuccess { get; set; }
    public bool RequiresMfa { get; set; }
    public string? Message { get; set; }
    public Models.User? User { get; set; }
    public string? Token { get; set; }
    public int FailedAttempts { get; set; }
    public int MaxAttempts { get; set; }
    public bool IsLockedOut { get; set; }
    public DateTime? LockoutEndTime { get; set; }
}
