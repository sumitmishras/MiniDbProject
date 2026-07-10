using MiniDbProject.Constants;
using MiniDbProject.DTOs;
using MiniDbProject.Helpers;
using MiniDbProject.Logging;
using MiniDbProject.Models;

namespace MiniDbProject.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly ILoggerService _logger;
    private User? _currentUser;

    public bool IsUserLoggedIn => _currentUser != null;
    public string? CurrentUsername => _currentUser?.Username;

    public AuthenticationService(ILoggerService logger)
    {
        _logger = logger;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var response = new LoginResponse
        {
            MaxAttempts = 5
        };

        try
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                response.Message = "Username and password are required.";
                await _logger.LogWarning("Login attempt with empty credentials.");
                return response;
            }

            await Task.Delay(50);

            if (IsDemoAccount(request.Username, request.Password))
            {
                _currentUser = CreateDemoUser(request.Username);

                if (request.RequiresMfa == null && _currentUser.IsMfaEnabled)
                {
                    response.RequiresMfa = true;
                    response.Message = "MFA code required.";
                    await _logger.LogInfo($"MFA required for user: {request.Username}");
                    return response;
                }

                if (_currentUser.IsMfaEnabled && !string.IsNullOrEmpty(request.MfaCode))
                {
                    bool mfaValid = await ValidateMfaCodeAsync(request.Username, request.MfaCode);
                    if (!mfaValid)
                    {
                        response.Message = "Invalid MFA code.";
                        await _logger.LogWarning($"Invalid MFA code attempt for user: {request.Username}");
                        return response;
                    }
                }

                response.IsSuccess = true;
                response.User = _currentUser;
                response.Message = "Login successful.";
                _currentUser.LastLoginDate = DateTime.Now;
                await _logger.LogSuccess($"User '{request.Username}' logged in successfully.");
                return response;
            }

            response.FailedAttempts = 1;
            response.Message = "Invalid username or password.";
            await _logger.LogWarning($"Failed login attempt for user: {request.Username}");
            return response;
        }
        catch (Exception ex)
        {
            await _logger.LogError($"Login error for user '{request.Username}'", ex);
            response.Message = "An error occurred during login. Please try again.";
            return response;
        }
    }

    public async Task LogoutAsync(string username)
    {
        _currentUser = null;
        await _logger.LogInfo($"User '{username}' logged out.");
    }

    public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
    {
        try
        {
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                await _logger.LogWarning("Password change failed: passwords do not match.");
                return false;
            }

            if (!PasswordHelper.IsStrongPassword(request.NewPassword))
            {
                await _logger.LogWarning("Password change failed: password is not strong enough.");
                return false;
            }

            await Task.Delay(50);
            await _logger.LogSuccess($"Password changed successfully for user '{request.Username}'.");
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogError($"Password change error for user '{request.Username}'", ex);
            return false;
        }
    }

    public async Task<bool> ValidateMfaCodeAsync(string username, string code)
    {
        await Task.Delay(30);
        if (code == "123456" || code == "000000")
        {
            await _logger.LogWarning($"MFA validation failed for user '{username}'.");
            return false;
        }
        await _logger.LogInfo($"MFA validation successful for user '{username}'.");
        return true;
    }

    public Task<User?> GetCurrentUserAsync()
    {
        return Task.FromResult(_currentUser);
    }

    private bool IsDemoAccount(string username, string password)
    {
        return (username == "admin" && password == "Admin@123") ||
               (username == "user" && password == "User@123") ||
               (username == "demo" && password == "Demo@123");
    }

    private User CreateDemoUser(string username)
    {
        return new User
        {
            UserId = 1,
            Username = username,
            FullName = username switch
            {
                "admin" => "Administrator",
                "user" => "Standard User",
                "demo" => "Demo User",
                _ => username
            },
            Email = $"{username}@example.com",
            IsMfaEnabled = false,
            LastLoginDate = DateTime.Now.AddDays(-1),
            LastLoginIp = "192.168.1.100",
            PhoneNumber = "+1-555-0100",
            Department = "IT",
            Role = username == "admin" ? "Administrator" : "User",
            CreatedDate = DateTime.Now.AddMonths(-3)
        };
    }
}
