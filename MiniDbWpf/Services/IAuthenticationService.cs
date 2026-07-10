using MiniDbWpf.Models;

namespace MiniDbWpf.Services;

public interface IAuthenticationService
{
    User? CurrentUser { get; }
    bool RequiresMfa { get; }
    Task<User?> LoginAsync(string username, string password, string? mfaCode = null);
    Task LogoutAsync();
}
