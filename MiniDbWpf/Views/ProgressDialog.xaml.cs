using System.Windows;
using MiniDbWpf.Services;
using MiniDbWpf.ViewModels;

namespace MiniDbWpf.Views;

public partial class ProgressDialog : Window
{
    private readonly ProgressViewModel _vm;

    public ProgressDialog(IDatabaseService database, ILoggerService logger,
        string server, string databaseName, string scriptPath)
    {
        InitializeComponent();
        _vm = new ProgressViewModel(database, logger);
        DataContext = _vm;
        Loaded += async (_, _) => await _vm.StartAsync(server, databaseName, scriptPath);
    }

    private async void BtnAction_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.IsRunning)
        {
            _vm.CancelCommand.Execute(null);
        }
        else
        {
            Close();
        }
    }
}
