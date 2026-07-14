using System.Windows;
using System.Windows.Controls;
using MiniDbWpf.ViewModels;

namespace MiniDbWpf.Views;

public partial class CreateDatabaseView : UserControl
{
    public CreateDatabaseView()
    {
        InitializeComponent();
    }

    private async void BtnValidate_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateDatabaseViewModel vm) return;
        vm.SourcePassword = pwSource.Password;
        await vm.ValidateCommand.ExecuteAsync(null);
    }

    private async void BtnCreate_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not CreateDatabaseViewModel vm) return;
        vm.SourcePassword = pwSource.Password;
        await vm.CreateDatabaseCommand.ExecuteAsync(null);
    }
}
