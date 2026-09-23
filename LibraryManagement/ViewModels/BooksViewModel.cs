using System.Collections.ObjectModel;
using System.Windows;
using LibraryManagement.Infrastructure;
using LibraryManagement.Models;

namespace LibraryManagement.ViewModels;

public sealed class BooksViewModel : ObservableObject, IRefreshable
{
    private string _searchText = string.Empty;
    private Book? _selectedBook;
    private string _isbn = "";
    private string _title = "";
    private string _author = "";
    private string _publisher = "";
    private string _publishYear = DateTime.Now.Year.ToString();
    private string _quantity = "1";
    private string _price = "0";
    private string _shelf = "A-01";
    private Category? _selectedCategory;
    private int? _editingId;
    private string _formTitle = "Thêm sách mới";

    public ObservableCollection<Book> Books { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();
    public string SearchText { get => _searchText; set { if (SetProperty(ref _searchText, value)) SearchCommand.RaiseCanExecuteChanged(); } }
    public Book? SelectedBook { get => _selectedBook; set { if (SetProperty(ref _selectedBook, value) && value is not null) LoadSelected(value); } }
    public string ISBN { get => _isbn; set => SetProperty(ref _isbn, value); }
    public string Title { get => _title; set => SetProperty(ref _title, value); }
    public string Author { get => _author; set => SetProperty(ref _author, value); }
    public string Publisher { get => _publisher; set => SetProperty(ref _publisher, value); }
    public string PublishYear { get => _publishYear; set => SetProperty(ref _publishYear, value); }
    public string Quantity { get => _quantity; set => SetProperty(ref _quantity, value); }
    public string Price { get => _price; set => SetProperty(ref _price, value); }
    public string Shelf { get => _shelf; set => SetProperty(ref _shelf, value); }
    public Category? SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }
    public int StockCount => Books.Sum(x => x.Quantity);
    public int AvailableCount => Books.Sum(x => x.AvailableQuantity);
    public string FormTitle { get => _formTitle; private set => SetProperty(ref _formTitle, value); }

    public RelayCommand SearchCommand { get; }
    public RelayCommand NewCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public BooksViewModel()
    {
        SearchCommand = new RelayCommand(_ => Refresh());
        NewCommand = new RelayCommand(_ => ClearForm());
        SaveCommand = new RelayCommand(async _ => await SaveAsync());
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedBook is not null);
        Refresh();
        LoadCategories();
    }

    public async void Refresh()
    {
        try
        {
            var data = await App.LibraryService.GetBooksAsync(SearchText);
            Books.Clear(); foreach (var x in data) Books.Add(x);
            OnPropertyChanged(nameof(StockCount)); OnPropertyChanged(nameof(AvailableCount));
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Books", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private async void LoadCategories()
    {
        try { Categories.Clear(); foreach (var x in await App.LibraryService.GetCategoriesAsync()) Categories.Add(x); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Books", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void LoadSelected(Book b)
    {
        _editingId = b.BookId; FormTitle = "Chỉnh sửa thông tin sách";
        ISBN = b.ISBN; Title = b.Title; Author = b.Author; Publisher = b.Publisher ?? "";
        PublishYear = b.PublishYear.ToString(); Quantity = b.Quantity.ToString(); Price = b.Price.ToString("0.##"); Shelf = b.ShelfLocation;
        SelectedCategory = Categories.FirstOrDefault(x => x.CategoryId == b.CategoryId);
    }

    private void ClearForm()
    {
        _editingId = null; FormTitle = "Thêm sách mới"; ISBN = Title = Author = Publisher = "";
        PublishYear = DateTime.Now.Year.ToString(); Quantity = "1"; Price = "0"; Shelf = "A-01"; SelectedCategory = Categories.FirstOrDefault();
        SelectedBook = null; DeleteCommand.RaiseCanExecuteChanged();
    }

    private async Task SaveAsync()
    {
        try
        {
            if (SelectedCategory is null || string.IsNullOrWhiteSpace(ISBN) || string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(Author))
                throw new InvalidOperationException("Vui lòng nhập đầy đủ ISBN, tên sách, tác giả và thể loại.");
            if (!int.TryParse(PublishYear, out var year) || year < 1000 || year > DateTime.Now.Year) throw new InvalidOperationException("Năm xuất bản không hợp lệ.");
            if (!int.TryParse(Quantity, out var qty) || qty <= 0) throw new InvalidOperationException("Số lượng phải lớn hơn 0.");
            if (!decimal.TryParse(Price, out var price) || price < 0) throw new InvalidOperationException("Giá không hợp lệ.");
            var b = new Book { ISBN=ISBN.Trim(), Title=Title.Trim(), Author=Author.Trim(), Publisher=Publisher.Trim(), PublishYear=year, Quantity=qty, Price=price, ShelfLocation=Shelf.Trim(), CategoryId=SelectedCategory.CategoryId };
            if (_editingId is null) b.AvailableQuantity = qty;
            await App.LibraryService.SaveBookAsync(b, _editingId);
            MessageBox.Show(_editingId is null ? "Đã thêm sách thành công." : "Đã cập nhật sách thành công.", "Books", MessageBoxButton.OK, MessageBoxImage.Information);
            Refresh(); ClearForm();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Không thể lưu", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }

    private async Task DeleteAsync()
    {
        if (SelectedBook is null) return;
        if (MessageBox.Show($"Vô hiệu hóa sách '{SelectedBook.Title}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try { await App.LibraryService.DeleteBookAsync(SelectedBook.BookId); Refresh(); ClearForm(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
}
