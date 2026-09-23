using System.Collections.ObjectModel;
using System.Windows;
using LibraryManagement.Infrastructure;
using LibraryManagement.Models;

namespace LibraryManagement.ViewModels;

public sealed class ReadersViewModel : ObservableObject, IRefreshable
{
    private string _searchText = "";
    private Reader? _selectedReader;
    private int? _editingId;
    private string _cardNumber = "";
    private string _fullName = "";
    private string _dateOfBirth = "01/01/2005";
    private string _phone = "";
    private string _email = "";
    private string _address = "";
    private string _formTitle = "Thêm độc giả mới";

    public ObservableCollection<Reader> Readers { get; } = new();
    public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }
    public Reader? SelectedReader { get => _selectedReader; set { if (SetProperty(ref _selectedReader, value) && value is not null) LoadSelected(value); } }
    public string CardNumber { get => _cardNumber; set => SetProperty(ref _cardNumber, value); }
    public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }
    public string DateOfBirth { get => _dateOfBirth; set => SetProperty(ref _dateOfBirth, value); }
    public string Phone { get => _phone; set => SetProperty(ref _phone, value); }
    public string Email { get => _email; set => SetProperty(ref _email, value); }
    public string Address { get => _address; set => SetProperty(ref _address, value); }
    public string FormTitle { get => _formTitle; private set => SetProperty(ref _formTitle, value); }

    public RelayCommand SearchCommand { get; }
    public RelayCommand NewCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand DeleteCommand { get; }

    public ReadersViewModel()
    {
        SearchCommand = new RelayCommand(_ => Refresh());
        NewCommand = new RelayCommand(_ => ClearForm());
        SaveCommand = new RelayCommand(async _ => await SaveAsync());
        DeleteCommand = new RelayCommand(async _ => await DeleteAsync(), _ => SelectedReader is not null);
        Refresh();
    }

    public async void Refresh()
    {
        try { Readers.Clear(); foreach (var x in await App.LibraryService.GetReadersAsync(SearchText)) Readers.Add(x); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Readers", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void LoadSelected(Reader r)
    {
        _editingId = r.ReaderId; FormTitle = "Chỉnh sửa thông tin độc giả"; CardNumber = r.CardNumber; FullName = r.FullName; DateOfBirth = r.DateOfBirth.ToString("dd/MM/yyyy"); Phone = r.Phone ?? ""; Email = r.Email ?? ""; Address = r.Address ?? ""; DeleteCommand.RaiseCanExecuteChanged();
    }
    private void ClearForm()
    {
        _editingId=null; FormTitle="Thêm độc giả mới"; CardNumber=""; FullName=""; DateOfBirth=DateTime.Today.AddYears(-18).ToString("dd/MM/yyyy"); Phone=""; Email=""; Address=""; SelectedReader=null; DeleteCommand.RaiseCanExecuteChanged();
    }
    private async Task SaveAsync()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(CardNumber) || string.IsNullOrWhiteSpace(FullName)) throw new InvalidOperationException("Vui lòng nhập mã thẻ và họ tên.");
            if (!DateTime.TryParse(DateOfBirth, out var dob)) throw new InvalidOperationException("Ngày sinh không hợp lệ.");
            if (dob.Date >= DateTime.Today) throw new InvalidOperationException("Ngày sinh phải ở trong quá khứ.");
            var r = new Reader { CardNumber=CardNumber.Trim(), FullName=FullName.Trim(), DateOfBirth=dob, Phone=Phone.Trim(), Email=Email.Trim(), Address=Address.Trim() };
            await App.LibraryService.SaveReaderAsync(r, _editingId);
            MessageBox.Show("Đã lưu thông tin độc giả.", "Readers", MessageBoxButton.OK, MessageBoxImage.Information);
            Refresh(); ClearForm();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Không thể lưu", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
    private async Task DeleteAsync()
    {
        if (SelectedReader is null) return;
        if (MessageBox.Show($"Vô hiệu hóa độc giả '{SelectedReader.FullName}'?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        try { await App.LibraryService.DeleteReaderAsync(SelectedReader.ReaderId); Refresh(); ClearForm(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Warning); }
    }
}
