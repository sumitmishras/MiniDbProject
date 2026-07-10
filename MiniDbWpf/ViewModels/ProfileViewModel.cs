using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniDbWpf.Models;
using MiniDbWpf.Services;

namespace MiniDbWpf.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IProfileService _profile;
    private readonly IAuthenticationService _auth;
    private readonly ILoggerService _logger;

    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _fullName = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _role = string.Empty;
    [ObservableProperty] private bool _isMfaEnabled;
    [ObservableProperty] private string _mfaSecret = string.Empty;

    [ObservableProperty] private string _currentPassword = string.Empty;
    [ObservableProperty] private string _newPassword = string.Empty;
    [ObservableProperty] private string _confirmPassword = string.Empty;

    [ObservableProperty] private string _profileStatus = string.Empty;
    [ObservableProperty] private string _passwordStatus = string.Empty;
    [ObservableProperty] private string _mfaStatus = string.Empty;

    public ProfileViewModel(IProfileService profile, IAuthenticationService auth, ILoggerService logger)
    {
        _profile = profile; _auth = auth; _logger = logger;
        _ = LoadProfile();
    }

    private async Task LoadProfile()
    {
        var user = _auth.CurrentUser;
        if (user == null) return;
        var p = await _profile.GetProfileAsync(user.Username);
        if (p == null) return;
        Username = p.Username; FullName = p.FullName; Email = p.Email;
        Role = p.Role; IsMfaEnabled = p.IsMfaEnabled;
    }

    [RelayCommand]
    private async Task SaveProfile()
    {
        var user = _auth.CurrentUser;
        if (user == null) return;
        user.FullName = FullName; user.Email = Email;
        var ok = await _profile.UpdateProfileAsync(user);
        ProfileStatus = ok ? "Profile updated." : "Update failed.";
        if (ok) await _logger.LogSuccess("Profile updated.");
    }

    [RelayCommand]
    private async Task ChangePassword()
    {
        if (NewPassword != ConfirmPassword)
        {
            PasswordStatus = "Passwords do not match!";
            return;
        }
        if (NewPassword.Length < 6)
        {
            PasswordStatus = "Password too short!";
            return;
        }
        var ok = await _profile.ChangePasswordAsync(Username, CurrentPassword, NewPassword);
        PasswordStatus = ok ? "Password changed." : "Current password is wrong.";
        if (ok)
        {
            CurrentPassword = NewPassword = ConfirmPassword = string.Empty;
            await _logger.LogSuccess("Password changed.");
        }
    }

    [RelayCommand]
    private async Task SetupMfa()
    {
        MfaSecret = await _profile.GenerateMfaSecretAsync(Username);
        var ok = await _profile.EnableMfaAsync(Username, MfaSecret);
        IsMfaEnabled = ok;
        MfaStatus = ok ? "MFA enabled. Secret: " + MfaSecret : "MFA setup failed.";
        if (ok) await _logger.LogSuccess("MFA enabled.");
    }

    [RelayCommand]
    private async Task DisableMfa()
    {
        var ok = await _profile.DisableMfaAsync(Username);
        IsMfaEnabled = !ok;
        MfaSecret = string.Empty;
        MfaStatus = ok ? "MFA disabled." : "Failed to disable MFA.";
        if (ok) await _logger.LogSuccess("MFA disabled.");
    }
}
