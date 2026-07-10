using System.Windows;
using MiniDbWpf.Services;
using MiniDbWpf.ViewModels;

namespace MiniDbWpf.Views;

public partial class MainWindow : Window
{
    public MainWindow(IAuthenticationService auth, ILoggerService logger, IProfileService profile, IDatabaseService database)
    {
        InitializeComponent();
        DataContext = new MainViewModel(auth, logger, profile, database);
    }
}
