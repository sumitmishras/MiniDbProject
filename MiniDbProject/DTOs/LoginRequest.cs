namespace MiniDbProject.DTOs;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? MfaCode { get; set; }
    public bool? RequiresMfa { get; set; }
}
