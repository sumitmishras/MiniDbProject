using MiniDbProject.DTOs;
using MiniDbProject.Models;

namespace MiniDbProject.Services;

public interface IProfileService
{
    Task<User?> GetProfileAsync(string username);
    Task<bool> UpdateProfileAsync(ProfileUpdateRequest request);
    Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    Task<bool> ToggleMfaAsync(string username, bool enable);
    Task<string> GenerateMfaSecretAsync(string username);
    Task<DateTime?> GetLastLoginAsync(string username);
}
