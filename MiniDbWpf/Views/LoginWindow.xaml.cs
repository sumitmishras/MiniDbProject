using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using MiniDbWpf.Services;

namespace MiniDbWpf.Views;

public partial class LoginWindow : Window
{
    private readonly IAuthenticationService _auth;
    private readonly ILoggerService _logger;

    public LoginWindow(IAuthenticationService auth, ILoggerService logger)
    {
        InitializeComponent();
        _auth = auth; _logger = logger;
        KeyDown += (_, e) => { if (e.Key == System.Windows.Input.Key.Escape) Close(); };
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

    private async void BtnLogin_Click(object sender, RoutedEventArgs e)
    {
        btnLogin.IsEnabled = false;
        lblError.Visibility = Visibility.Collapsed;
        lblInfo.Visibility = Visibility.Collapsed;

        var username = txtUsername.Text.Trim();
        var password = txtPassword.Password;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ShowError("Please enter username and password.");
            btnLogin.IsEnabled = true;
            return;
        }

        lblInfo.Text = "Authenticating...";
        lblInfo.Visibility = Visibility.Visible;

        var user = await _auth.LoginAsync(username, password);

        if (_auth.RequiresMfa)
        {
            lblInfo.Text = "MFA code required. Enter 6-digit code:";
            var mfa = new MfaDialog();
            if (mfa.ShowDialog() == true)
                user = await _auth.LoginAsync(username, password, mfa.MfaCode);
        }

        if (user != null)
        {
            await _logger.LogSuccess($"User '{username}' logged in.");
            var mainWindow = new MainWindow(
                _auth,
                _logger,
                App.ServiceProvider.GetRequiredService<IProfileService>(),
                App.ServiceProvider.GetRequiredService<IDatabaseService>());
            mainWindow.Show();
            Close();
        }
        else
        {
            ShowError(_auth.RequiresMfa ? "Invalid MFA code." : "Invalid credentials.");
        }

        btnLogin.IsEnabled = true;
    }

    private void ShowError(string msg)
    {
        lblError.Text = msg;
        lblError.Visibility = Visibility.Visible;
        lblInfo.Visibility = Visibility.Collapsed;
    }
}
