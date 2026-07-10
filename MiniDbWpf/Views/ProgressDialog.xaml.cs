using System.Windows;
using MiniDbWpf.Services;
using MiniDbWpf.ViewModels;

namespace MiniDbWpf.Views;

public partial class ProgressDialog : Window
{
    private readonly ProgressViewModel _vm;

    public ProgressDialog(IDatabaseService database, ILoggerService logger,
        string sourceServer, string sourceDatabase,
        string? sourceUser, string? sourcePassword,
        string destinationServer, string destinationDatabase,
        DateTime fromDate, DateTime toDate, bool debug)
    {
        InitializeComponent();
        _vm = new ProgressViewModel(database, logger);
        DataContext = _vm;
        Loaded += async (_, _) =>
            await _vm.StartAsync(sourceServer, sourceDatabase,
                sourceUser, sourcePassword,
                destinationServer, destinationDatabase,
                fromDate, toDate, debug);
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
