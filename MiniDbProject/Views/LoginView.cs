using MiniDbProject.Authentication;
using MiniDbProject.Constants;
using MiniDbProject.DTOs;
using MiniDbProject.Helpers;
using MiniDbProject.Resources;

namespace MiniDbProject.Views;

public class LoginView : IView
{
    private readonly IAuthenticationService _authService;
    public string Name => "Login";

    public LoginView(IAuthenticationService authService)
    {
        _authService = authService;
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            ConsoleHelper.ClearScreen();
            ConsoleHelper.DisplayBanner(AppConstants.AppName, AppConstants.AppVersion, AppConstants.Author);

            ConsoleRenderer.DrawHeader(Messages.Login.Title, "Please enter your credentials to continue");

            ConsoleHelper.WriteLineColored("");
            ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.White}Username: admin | user | demo{AppConstants.Colors.Reset}");
            ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.White}Password: Admin@123 | User@123 | Demo@123{AppConstants.Colors.Reset}");
            ConsoleHelper.WriteLineColored($"  {AppConstants.Colors.Dim}These are demo accounts for testing purposes.{AppConstants.Colors.Reset}");
            ConsoleHelper.WriteLineColored("");

            string username = InputHelper.ReadString(Messages.Login.UsernamePrompt, required: true, maxLength: 50);
            if (string.IsNullOrWhiteSpace(username)) continue;

            string password = InputHelper.ReadPassword(Messages.Login.PasswordPrompt);
            if (string.IsNullOrWhiteSpace(password)) continue;

            ConsoleHelper.WriteLineColored("");
            using var spinner = new Utilities.ConsoleSpinner("Authenticating...");
            spinner.Start();

            var request = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var response = await _authService.LoginAsync(request);
            spinner.Stop();

            if (response.RequiresMfa)
            {
                ConsoleHelper.WriteLineColored($"\n {AppConstants.Icons.Lock} {Messages.Login.MfaRequired}", AppConstants.Colors.Warning);
                string mfaCode = InputHelper.ReadString(Messages.Login.MfaPrompt, required: true, maxLength: 10);
                request.MfaCode = mfaCode;
                request.RequiresMfa = true;

                spinner.Start();
                response = await _authService.LoginAsync(request);
                spinner.Stop();
            }

            if (response.IsSuccess)
            {
                ConsoleHelper.DisplayNotification(string.Format(Messages.Login.Success, username), "success");
                ConsoleHelper.WaitForKey();
                return;
            }

            if (response.IsLockedOut)
            {
                ConsoleHelper.DisplayNotification(
                    string.Format(Messages.Login.LockedOut, 15), "error");
            }
            else
            {
                ConsoleHelper.DisplayNotification(Messages.Login.Failed, "error");
                ConsoleHelper.WriteInfo($"Attempts remaining: {5 - response.FailedAttempts}");
            }

            ConsoleHelper.WaitForKey();
        }
    }
}
