using System.Windows;

namespace MiniDbWpf.Views;

public partial class MfaDialog : Window
{
    public string MfaCode => txtCode.Text.Trim();

    public MfaDialog()
    {
        InitializeComponent();
        Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
    }

    private void BtnVerify_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    private void BtnCancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
