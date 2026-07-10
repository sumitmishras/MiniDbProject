using MiniDbProject.Authentication;
using MiniDbProject.DTOs;
using MiniDbProject.Helpers;
using MiniDbProject.Logging;
using MiniDbProject.Models;

namespace MiniDbProject.Services;

public class ProfileService : IProfileService
{
    private readonly ILoggerService _logger;

    public ProfileService(ILoggerService logger)
    {
        _logger = logger;
    }

    public async Task<User?> GetProfileAsync(string username)
    {
        await _logger.LogInfo($"Fetching profile for user: {username}");
        await Task.Delay(50);

        return new User
        {
            UserId = 1,
            Username = username,
            FullName = "Demo User",
            Email = $"{username}@example.com",
            PhoneNumber = "+1-555-0100",
            Department = "IT",
            Role = "User",
            IsMfaEnabled = false,
            LastLoginDate = DateTime.Now.AddDays(-1),
            LastLoginIp = "192.168.1.100",
            CreatedDate = DateTime.Now.AddMonths(-3),
            IsActive = true
        };
    }

    public async Task<bool> UpdateProfileAsync(ProfileUpdateRequest request)
    {
        await _logger.LogInfo($"Updating profile for user: {request.Username}");
        await Task.Delay(100);
        await _logger.LogSuccess($"Profile updated for user '{request.Username}'.");
        return true;
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        await _logger.LogInfo($"Changing password for user: {request.Username}");

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
            string.IsNullOrWhiteSpace(request.NewPassword))
        {
            await _logger.LogWarning("Password change failed: empty fields.");
            return false;
        }

        if (request.NewPassword != request.ConfirmNewPassword)
        {
            await _logger.LogWarning("Password change failed: passwords do not match.");
            return false;
        }

        if (!PasswordHelper.IsStrongPassword(request.NewPassword))
        {
            await _logger.LogWarning("Password change failed: weak password.");
            return false;
        }

        await Task.Delay(100);
        await _logger.LogSuccess($"Password changed successfully for user '{request.Username}'.");
        return true;
    }

    public async Task<bool> ToggleMfaAsync(string username, bool enable)
    {
        string action = enable ? "enable" : "disable";
        await _logger.LogInfo($"User '{username}' requested to {action} MFA.");
        await Task.Delay(50);
        string message = enable ? "MFA enabled" : "MFA disabled";
        await _logger.LogSuccess($"{message} for user '{username}'.");
        return true;
    }

    public async Task<string> GenerateMfaSecretAsync(string username)
    {
        await _logger.LogInfo($"Generating MFA secret for user: {username}");
        await Task.Delay(100);
        string secret = Guid.NewGuid().ToString("N").ToUpperInvariant()[..16];
        await _logger.LogInfo($"MFA secret generated for user '{username}'.");
        return secret;
    }

    public async Task<DateTime?> GetLastLoginAsync(string username)
    {
        await Task.Delay(30);
        return DateTime.Now.AddDays(-1);
    }
}
