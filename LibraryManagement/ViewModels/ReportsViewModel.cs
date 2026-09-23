using System.Collections.ObjectModel;
using LibraryManagement.Infrastructure;
using LibraryManagement.Models;

namespace LibraryManagement.ViewModels;

public sealed class ReportsViewModel : ObservableObject, IRefreshable
{
    private int _totalBooks;
    private int _available;
    private int _borrowed;
    private int _overdue;
    private decimal _fines;
    private string _selectedPeriod = "Tất cả";

    public int TotalBooks { get => _totalBooks; private set => SetProperty(ref _totalBooks, value); }
    public int Available { get => _available; private set => SetProperty(ref _available, value); }
    public int Borrowed { get => _borrowed; private set => SetProperty(ref _borrowed, value); }
    public int Overdue { get => _overdue; private set => SetProperty(ref _overdue, value); }
    public decimal Fines { get => _fines; private set => SetProperty(ref _fines, value); }
    public string SelectedPeriod { get => _selectedPeriod; set { if (SetProperty(ref _selectedPeriod, value)) Refresh(); } }
    public ObservableCollection<Loan> RecentLoans { get; } = new();
    public ObservableCollection<ReportMetric> Metrics { get; } = new();

    public ReportsViewModel() => Refresh();
    public async void Refresh()
    {
        try
        {
            var s = await App.LibraryService.GetDashboardStatsAsync();
            TotalBooks=s.TotalBooks; Available=s.AvailableBooks; Borrowed=s.ActiveLoans; Overdue=s.OverdueLoans; Fines=s.TotalFines;
            RecentLoans.Clear(); foreach(var x in (await App.LibraryService.GetLoansAsync()).Take(10)) RecentLoans.Add(x);
            Metrics.Clear();
            Metrics.Add(new ReportMetric("Sách có sẵn", Available, TotalBooks == 0 ? 0 : Available * 100.0 / TotalBooks));
            Metrics.Add(new ReportMetric("Đang lưu hành", Borrowed, TotalBooks == 0 ? 0 : Borrowed * 100.0 / TotalBooks));
            Metrics.Add(new ReportMetric("Quá hạn", Overdue, Borrowed == 0 ? 0 : Overdue * 100.0 / Borrowed));
        } catch { }
    }
}

public record ReportMetric(string Name, int Value, double Percentage);
