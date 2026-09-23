using System.Collections.ObjectModel;
using System.Windows;
using LibraryManagement.Infrastructure;
using LibraryManagement.Models;

namespace LibraryManagement.ViewModels;

public sealed class LoansViewModel : ObservableObject, IRefreshable
{
    private Reader? _selectedReader;
    private Book? _selectedBook;
    private DateTime _dueDate = DateTime.Today.AddDays(14);
    private string _note = "";
    private Loan? _selectedLoan;
    private string _filter = "Tất cả";

    public ObservableCollection<Reader> Readers { get; } = new();
    public ObservableCollection<Book> Books { get; } = new();
    public ObservableCollection<Loan> Loans { get; } = new();
    public Reader? SelectedReader { get => _selectedReader; set => SetProperty(ref _selectedReader, value); }
    public Book? SelectedBook { get => _selectedBook; set => SetProperty(ref _selectedBook, value); }
    public DateTime DueDate { get => _dueDate; set => SetProperty(ref _dueDate, value); }
    public string Note { get => _note; set => SetProperty(ref _note, value); }
    public Loan? SelectedLoan { get => _selectedLoan; set => SetProperty(ref _selectedLoan, value); }
    public string Filter { get => _filter; set { if (SetProperty(ref _filter, value)) Refresh(); } }
    public string ActiveSummary => $"{Loans.Count(x => x.Status != LoanStatus.Returned)} đang mượn · {Loans.Count(x => x.Status == LoanStatus.Overdue)} quá hạn";

    public RelayCommand BorrowCommand { get; }
    public RelayCommand ReturnCommand { get; }
    public RelayCommand RefreshCommand { get; }

    public LoansViewModel()
    {
        BorrowCommand = new RelayCommand(async _ => await BorrowAsync());
        ReturnCommand = new RelayCommand(async _ => await ReturnAsync(), _ => SelectedLoan is not null && SelectedLoan.Status != LoanStatus.Returned);
        RefreshCommand = new RelayCommand(_ => Refresh());
        Refresh(); LoadSelectors();
    }

    public async void Refresh()
    {
        try
        {
            var data = await App.LibraryService.GetLoansAsync();
            Loans.Clear(); foreach (var x in ApplyFilter(data)) Loans.Add(x);
            OnPropertyChanged(nameof(ActiveSummary)); ReturnCommand.RaiseCanExecuteChanged();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Borrow & Return", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private IEnumerable<Loan> ApplyFilter(List<Loan> data) => Filter switch
    {
        "Đang mượn" => data.Where(x => x.Status == LoanStatus.Borrowed),
        "Quá hạn" => data.Where(x => x.Status == LoanStatus.Overdue),
        "Đã trả" => data.Where(x => x.Status == LoanStatus.Returned),
        _ => data
    };

    private async void LoadSelectors()
    {
        try
        {
            Readers.Clear(); foreach (var x in await App.LibraryService.GetReadersAsync()) Readers.Add(x);
            Books.Clear(); foreach (var x in await App.LibraryService.GetBooksAsync()) if (x.AvailableQuantity > 0) Books.Add(x);
            SelectedReader = Readers.FirstOrDefault(); SelectedBook = Books.FirstOrDefault();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Borrow & Return", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async Task BorrowAsync()
    {
        try
        {
            if (SelectedReader is null || SelectedBook is null) throw new InvalidOperationException("Chọn độc giả và sách cần mượn.");
            await App.LibraryService.BorrowAsync(SelectedReader.ReaderId, SelectedBook.BookId, DueDate, Note);
            MessageBox.Show("Tạo phiếu mượn thành công.", "Borrow & Return", MessageBoxButton.OK, MessageBoxImage.Information);
            Note=""; Refresh(); LoadSelectors();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Không thể mượn sách", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private async Task ReturnAsync()
    {
        if (SelectedLoan is null) return;
        if (MessageBox.Show($"Xác nhận trả phiếu {SelectedLoan.LoanCode}?", "Xác nhận trả sách", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try
        {
            await App.LibraryService.ReturnAsync(SelectedLoan.LoanId);
            MessageBox.Show("Đã ghi nhận trả sách. Tiền phạt (nếu có) được tính tự động 5.000đ/ngày.", "Borrow & Return", MessageBoxButton.OK, MessageBoxImage.Information);
            Refresh(); LoadSelectors();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Không thể trả sách", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
}
