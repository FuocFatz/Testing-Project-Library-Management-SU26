using System.Collections.ObjectModel;
using LibraryManagement.Infrastructure;
using LibraryManagement.Services;

namespace LibraryManagement.ViewModels;

public sealed class DashboardViewModel : ObservableObject, IRefreshable
{
    private int _totalBooks;
    private int _availableBooks;
    private int _totalReaders;
    private int _activeLoans;
    private int _overdueLoans;
    private decimal _totalFines;
    private string _statusText = "Đang tải dữ liệu...";

    public int TotalBooks { get => _totalBooks; private set => SetProperty(ref _totalBooks, value); }
    public int AvailableBooks { get => _availableBooks; private set => SetProperty(ref _availableBooks, value); }
    public int TotalReaders { get => _totalReaders; private set => SetProperty(ref _totalReaders, value); }
    public int ActiveLoans { get => _activeLoans; private set => SetProperty(ref _activeLoans, value); }
    public int OverdueLoans { get => _overdueLoans; private set => SetProperty(ref _overdueLoans, value); }
    public decimal TotalFines { get => _totalFines; private set => SetProperty(ref _totalFines, value); }
    public double AvailabilityRate => TotalBooks == 0 ? 0 : AvailableBooks * 100.0 / TotalBooks;
    public ObservableCollection<TopBook> TopBooks { get; } = new();
    public string StatusText { get => _statusText; private set => SetProperty(ref _statusText, value); }

    public DashboardViewModel() => Refresh();

    public async void Refresh()
    {
        try
        {
            await App.LibraryService.UpdateOverdueStatusesAsync();
            var s = await App.LibraryService.GetDashboardStatsAsync();
            TotalBooks = s.TotalBooks;
            AvailableBooks = s.AvailableBooks;
            TotalReaders = s.TotalReaders;
            ActiveLoans = s.ActiveLoans;
            OverdueLoans = s.OverdueLoans;
            TotalFines = s.TotalFines;
            TopBooks.Clear(); foreach (var item in s.TopBooks) TopBooks.Add(item);
            OnPropertyChanged(nameof(AvailabilityRate));
            StatusText = $"Cập nhật lúc {DateTime.Now:HH:mm:ss}";
        }
        catch (Exception ex) { StatusText = $"Không thể tải dashboard: {ex.Message}"; }
    }
}
