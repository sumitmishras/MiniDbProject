using MiniDbProject.Authentication;
using MiniDbProject.Constants;
using MiniDbProject.DTOs;
using MiniDbProject.Helpers;
using MiniDbProject.Logging;
using MiniDbProject.Resources;
using MiniDbProject.Services;

namespace MiniDbProject.Views;

public class ProfileView : IView
{
    private readonly IProfileService _profileService;
    private readonly IAuthenticationService _authService;
    private readonly ILoggerService _logger;

    public string Name => "Profile";

    public ProfileView(IProfileService profileService, IAuthenticationService authService, ILoggerService logger)
    {
        _profileService = profileService;
        _authService = authService;
        _logger = logger;
    }

    public async Task ShowAsync()
    {
        var user = await _authService.GetCurrentUserAsync();
        if (user == null) return;

        bool stay = true;
        while (stay)
        {
            ConsoleHelper.ClearScreen();
            ConsoleRenderer.DrawHeader(Messages.Profile.Title);

            var info = new Dictionary<string, string>
            {
                { "Username", user.Username },
                { "Full Name", user.FullName },
                { "Email", user.Email },
                { "Phone", user.PhoneNumber ?? "N/A" },
                { "Department", user.Department ?? "N/A" },
                { "Role", user.Role ?? "User" },
                { "MFA Status", user.IsMfaEnabled ? $"{AppConstants.Colors.Success}Enabled{AppConstants.Colors.Reset}" : $"{AppConstants.Colors.Dim}Disabled{AppConstants.Colors.Reset}" },
                { "Last Login", user.LastLoginDate?.ToString("yyyy-MM-dd HH:mm") ?? "N/A" },
                { "Last IP", user.LastLoginIp ?? "N/A" },
                { "Account Created", user.CreatedDate.ToString("yyyy-MM-dd") }
            };
            ConsoleRenderer.DrawInfoPanel("Account Information", info);

            ConsoleHelper.WriteLineColored("");
            ConsoleRenderer.DrawMenuTitle("Profile Options");
            ConsoleRenderer.DrawMenuItem("1", "Update Profile", "Edit your personal information");
            ConsoleRenderer.DrawMenuItem("2", "Change Password", "Update your login password");
            ConsoleRenderer.DrawMenuItem("3", "Toggle MFA", "Enable or disable multi-factor authentication");
            ConsoleRenderer.DrawMenuItem("B", "Back to Dashboard", "");

            ConsoleKey k;
            try { k = Console.ReadKey(true).Key; } catch { return; }
            switch (k)
            {
                case ConsoleKey.D1:
                    await UpdateProfileAsync(user);
                    break;
                case ConsoleKey.D2:
                    await ChangePasswordAsync(user);
                    break;
                case ConsoleKey.D3:
                    await ToggleMfaAsync(user);
                    break;
                case ConsoleKey.B:
                case ConsoleKey.Escape:
                    stay = false;
                    break;
            }
        }
    }

    private async Task UpdateProfileAsync(Models.User user)
    {
        ConsoleHelper.ClearScreen();
        ConsoleRenderer.DrawHeader("Update Profile");

        ConsoleHelper.WriteLineColored("");
        string fullName = InputHelper.ReadString($"Full Name [{user.FullName}]", required: true, defaultValue: user.FullName);
        string email = InputHelper.ReadString($"Email [{user.Email}]", required: true, defaultValue: user.Email);
        string phone = InputHelper.ReadString($"Phone [{user.PhoneNumber ?? ""}]", required: false, defaultValue: user.PhoneNumber);
        string dept = InputHelper.ReadString($"Department [{user.Department ?? ""}]", required: false, defaultValue: user.Department);

        if (!Helpers.ValidationHelper.IsValidEmail(email))
        {
            ConsoleHelper.WriteWarning("Invalid email format.");
            ConsoleHelper.WaitForKey();
            return;
        }

        var request = new ProfileUpdateRequest
        {
            Username = user.Username,
            FullName = fullName,
            Email = email,
            PhoneNumber = phone,
            Department = dept
        };

        using var spinner = new Utilities.ConsoleSpinner("Saving...");
        spinner.Start();
        bool success = await _profileService.UpdateProfileAsync(request);
        spinner.Stop();

        if (success)
        {
            ConsoleHelper.DisplayNotification(Messages.Profile.UpdateSuccess, "success");
        }
        ConsoleHelper.WaitForKey();
    }

    private async Task ChangePasswordAsync(Models.User user)
    {
        ConsoleHelper.ClearScreen();
        ConsoleRenderer.DrawHeader("Change Password");

        ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}Password must be at least 8 characters with uppercase, lowercase, digit, and special character.{AppConstants.Colors.Reset}");
        ConsoleHelper.WriteLineColored("");

        string currentPwd = InputHelper.ReadPassword("Current Password");
        string newPwd = InputHelper.ReadPassword("New Password");
        string confirmPwd = InputHelper.ReadPassword("Confirm New Password");

        if (string.IsNullOrWhiteSpace(currentPwd) || string.IsNullOrWhiteSpace(newPwd))
        {
            ConsoleHelper.WriteWarning("All fields are required.");
            ConsoleHelper.WaitForKey();
            return;
        }

        if (newPwd != confirmPwd)
        {
            ConsoleHelper.WriteWarning("New passwords do not match.");
            ConsoleHelper.WaitForKey();
            return;
        }

        string strength = PasswordHelper.GetPasswordStrengthMessage(newPwd);
        ConsoleHelper.WriteInfo($"Password strength: {strength}");

        if (!PasswordHelper.IsStrongPassword(newPwd))
        {
            ConsoleHelper.WriteWarning("Password is not strong enough.");
            ConsoleHelper.WaitForKey();
            return;
        }

        var request = new ChangePasswordRequest
        {
            Username = user.Username,
            CurrentPassword = currentPwd,
            NewPassword = newPwd,
            ConfirmNewPassword = confirmPwd
        };

        using var spinner = new Utilities.ConsoleSpinner("Changing password...");
        spinner.Start();
        bool success = await _profileService.ChangePasswordAsync(request);
        spinner.Stop();

        if (success)
        {
            ConsoleHelper.DisplayNotification(Messages.Profile.PasswordChangeSuccess, "success");
        }
        else
        {
            ConsoleHelper.DisplayNotification(Messages.Profile.PasswordChangeFailed, "error");
        }
        ConsoleHelper.WaitForKey();
    }

    private async Task ToggleMfaAsync(Models.User user)
    {
        ConsoleHelper.ClearScreen();
        ConsoleRenderer.DrawHeader("Multi-Factor Authentication");

        string currentStatus = user.IsMfaEnabled ? "enabled" : "disabled";
        ConsoleHelper.WriteLineColored($"  MFA is currently {currentStatus}.");
        ConsoleHelper.WriteLineColored("");

        bool enable = !user.IsMfaEnabled;
        string action = enable ? "enable" : "disable";
        if (!ConsoleHelper.PromptConfirmation($"Are you sure you want to {action} MFA?"))
            return;

        using var spinner = new Utilities.ConsoleSpinner("Processing...");
        spinner.Start();

        if (enable)
        {
            string secret = await _profileService.GenerateMfaSecretAsync(user.Username);
            spinner.Stop();
            ConsoleHelper.DisplayNotification(string.Format(Messages.Profile.MfaCode, secret), "info");
            ConsoleHelper.WriteLineColored("");
            ConsoleHelper.WriteInfo("Use an authenticator app like Google Authenticator or Microsoft Authenticator.");
            ConsoleHelper.WaitForKey("Press any key after setting up MFA in your authenticator app...");
            spinner.Start();
        }

        bool success = await _profileService.ToggleMfaAsync(user.Username, enable);
        spinner.Stop();

        if (success)
        {
            string msg = enable ? Messages.Profile.MfaEnabled : Messages.Profile.MfaDisabled;
            ConsoleHelper.DisplayNotification(msg, "success");
        }
        ConsoleHelper.WaitForKey();
    }
}
