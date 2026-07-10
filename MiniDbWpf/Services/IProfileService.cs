using MiniDbWpf.Models;

namespace MiniDbWpf.Services;

public interface IProfileService
{
    Task<User?> GetProfileAsync(string username);
    Task<bool> UpdateProfileAsync(User updated);
    Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword);
    Task<string> GenerateMfaSecretAsync(string username);
    Task<bool> EnableMfaAsync(string username, string secret);
    Task<bool> DisableMfaAsync(string username);
}
