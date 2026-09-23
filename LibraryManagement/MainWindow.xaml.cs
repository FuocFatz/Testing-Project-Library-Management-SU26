using System.Windows;
using LibraryManagement.ViewModels;

namespace LibraryManagement;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    private void Dashboard_Click(object sender, RoutedEventArgs e) => ((MainWindowViewModel)DataContext).ShowDashboard();
    private void Books_Click(object sender, RoutedEventArgs e) => ((MainWindowViewModel)DataContext).ShowBooks();
    private void Readers_Click(object sender, RoutedEventArgs e) => ((MainWindowViewModel)DataContext).ShowReaders();
    private void Loans_Click(object sender, RoutedEventArgs e) => ((MainWindowViewModel)DataContext).ShowLoans();
    private void Reports_Click(object sender, RoutedEventArgs e) => ((MainWindowViewModel)DataContext).ShowReports();
    private void Refresh_Click(object sender, RoutedEventArgs e) => ((MainWindowViewModel)DataContext).Refresh();
}
