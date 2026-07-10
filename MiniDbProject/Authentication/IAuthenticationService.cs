using MiniDbProject.DTOs;
using MiniDbProject.Models;

namespace MiniDbProject.Authentication;

public interface IAuthenticationService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task LogoutAsync(string username);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    Task<bool> ValidateMfaCodeAsync(string username, string code);
    Task<User?> GetCurrentUserAsync();
    bool IsUserLoggedIn { get; }
    string? CurrentUsername { get; }
}
