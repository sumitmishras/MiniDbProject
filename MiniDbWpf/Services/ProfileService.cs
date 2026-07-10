using MiniDbWpf.Models;
using MiniDbWpf.Helpers;

namespace MiniDbWpf.Services;

public class ProfileService : IProfileService
{
    private readonly List<User> _users;

    public ProfileService(IAuthenticationService auth)
    {
        _users =
        [
            new() { Username = "admin", FullName = "Administrator", Email = "admin@minidb.local", Role = "Admin",
                    PasswordHash = PasswordHelper.HashPassword("Admin@123"), LastLoginDate = DateTime.Now.AddDays(-1) },
            new() { Username = "user", FullName = "Standard User", Email = "user@minidb.local", Role = "User",
                    PasswordHash = PasswordHelper.HashPassword("User@123"), LastLoginDate = DateTime.Now.AddDays(-2) },
            new() { Username = "demo", FullName = "Demo User", Email = "demo@minidb.local", Role = "User",
                    PasswordHash = PasswordHelper.HashPassword("Demo@123"), LastLoginDate = DateTime.Now.AddDays(-3) }
        ];
        if (auth.CurrentUser != null)
        {
            var u = _users.First(x => x.Username == auth.CurrentUser.Username);
            u.LastLoginDate = auth.CurrentUser.LastLoginDate;
        }
    }

    public Task<User?> GetProfileAsync(string username)
    {
        var user = _users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user != null ? new User
        {
            Username = user.Username, FullName = user.FullName, Email = user.Email,
            Role = user.Role, IsMfaEnabled = user.IsMfaEnabled, LastLoginDate = user.LastLoginDate
        } : null);
    }

    public Task<bool> UpdateProfileAsync(User updated)
    {
        var user = _users.FirstOrDefault(u => u.Username == updated.Username);
        if (user == null) return Task.FromResult(false);
        user.FullName = updated.FullName;
        user.Email = updated.Email;
        return Task.FromResult(true);
    }

    public Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword)
    {
        var user = _users.FirstOrDefault(u => u.Username == username);
        if (user == null || !PasswordHelper.VerifyPassword(currentPassword, user.PasswordHash))
            return Task.FromResult(false);
        user.PasswordHash = PasswordHelper.HashPassword(newPassword);
        return Task.FromResult(true);
    }

    public Task<string> GenerateMfaSecretAsync(string username)
        => Task.FromResult("MFA-SECRET-KEY-123456");

    public Task<bool> EnableMfaAsync(string username, string secret)
    {
        var user = _users.FirstOrDefault(u => u.Username == username);
        if (user == null) return Task.FromResult(false);
        user.IsMfaEnabled = true;
        user.MfaSecret = secret;
        return Task.FromResult(true);
    }

    public Task<bool> DisableMfaAsync(string username)
    {
        var user = _users.FirstOrDefault(u => u.Username == username);
        if (user == null) return Task.FromResult(false);
        user.IsMfaEnabled = false;
        return Task.FromResult(true);
    }
}
