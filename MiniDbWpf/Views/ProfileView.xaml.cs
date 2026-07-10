using System.Windows;
using System.Windows.Controls;
using MiniDbWpf.ViewModels;

namespace MiniDbWpf.Views;

public partial class ProfileView : UserControl
{
    public ProfileView()
    {
        InitializeComponent();
    }

    private async void BtnChangePassword_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not ProfileViewModel vm) return;
        vm.CurrentPassword = pwCurrent.Password;
        vm.NewPassword = pwNew.Password;
        vm.ConfirmPassword = pwConfirm.Password;
        await vm.ChangePasswordCommand.ExecuteAsync(null);
        pwCurrent.Password = pwNew.Password = pwConfirm.Password = string.Empty;
    }
}
