using LibraryManagement.Infrastructure;

namespace LibraryManagement.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private object _currentView;
    public object CurrentView { get => _currentView; private set => SetProperty(ref _currentView, value); }

    public MainWindowViewModel()
    {
        _currentView = new DashboardViewModel();
    }

    public void ShowDashboard() => CurrentView = new DashboardViewModel();
    public void ShowBooks() => CurrentView = new BooksViewModel();
    public void ShowReaders() => CurrentView = new ReadersViewModel();
    public void ShowLoans() => CurrentView = new LoansViewModel();
    public void ShowReports() => CurrentView = new ReportsViewModel();
    public void Refresh() => (CurrentView as IRefreshable)?.Refresh();
}

public interface IRefreshable { void Refresh(); }
