using MiniDbWpf.Models;
using MiniDbWpf.Helpers;

namespace MiniDbWpf.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly List<User> _users;
    public User? CurrentUser { get; private set; }
    public bool RequiresMfa { get; private set; }

    public AuthenticationService()
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
    }

    public Task<User?> LoginAsync(string username, string password, string? mfaCode = null)
    {
        var user = _users.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
        {
            RequiresMfa = false;
            return Task.FromResult<User?>(null);
        }

        if (user.IsMfaEnabled)
        {
            if (mfaCode == null)
            {
                RequiresMfa = true;
                return Task.FromResult<User?>(null);
            }
            if (mfaCode != "123456")
            {
                RequiresMfa = false;
                return Task.FromResult<User?>(null);
            }
        }

        RequiresMfa = false;
        user.LastLoginDate = DateTime.Now;
        CurrentUser = user;
        return Task.FromResult<User?>(user);
    }

    public Task LogoutAsync()
    {
        CurrentUser = null;
        RequiresMfa = false;
        return Task.CompletedTask;
    }
}
