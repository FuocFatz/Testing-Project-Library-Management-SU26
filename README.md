# Library Management Pro — WPF / PRN212

Ứng dụng quản lý thư viện desktop xây dựng bằng **WPF .NET 8 + C# + Entity Framework Core + SQL Server** theo hướng MVVM.

## Tính năng
- Dashboard tổng quan: tổng sách, sách có sẵn, độc giả, phiếu đang mượn, quá hạn, tiền phạt.
- Books: tìm kiếm theo ISBN / tên sách / tác giả; thêm, sửa, vô hiệu hóa; quản lý thể loại, tồn kho và vị trí kệ.
- Readers: tìm kiếm độc giả; thêm, sửa, vô hiệu hóa thẻ.
- Borrow & Return: lập phiếu mượn, kiểm tra tồn kho, ngày trả, chuyển trạng thái quá hạn, trả sách và tính phạt.
- Reports: báo cáo nhanh, tỷ lệ có sẵn, lịch sử giao dịch gần nhất.
- Soft delete thông qua `IsActive`, hạn chế xóa cứng dữ liệu nghiệp vụ.

## Cấu trúc
```text
LibraryManagement/
  Data/             DbContext + database initializer
  Infrastructure/   MVVM base + RelayCommand
  Models/           Entity models
  Services/         Business/data service
  ViewModels/       Presentation logic
  Views/            WPF UserControls
  App.xaml          Theme + DataTemplates
  MainWindow.xaml   Main shell + navigation
  appsettings.json  SQL Server connection string
Database/
  LibraryManagementDB.sql
```

## Chạy bằng Visual Studio
1. Mở `LibraryManagement.sln` bằng Visual Studio 2022 có workload **.NET desktop development**.
2. Đảm bảo có SQL Server LocalDB (thường đi cùng Visual Studio).
3. Build solution và Run.
4. App tự tạo database `LibraryManagementDB` bằng `EnsureCreated()` và seed dữ liệu mẫu lần đầu.
5. Connection string nằm ở `LibraryManagement/appsettings.json`.

Connection mặc định:
```text
Server=(localdb)\\MSSQLLocalDB;Database=LibraryManagementDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True
```

Có thể thay bằng SQL Server thật, ví dụ:
```text
Server=.;Database=LibraryManagementDB;Trusted_Connection=True;TrustServerCertificate=True;
```

## Luồng nghiệp vụ chính
- **Borrow:** reader active + book active + available quantity > 0 -> tạo loan -> giảm tồn khả dụng.
- **Overdue:** khi app load dashboard/refresh, phiếu Borrowed có `DueDate < Today` chuyển thành `Overdue`.
- **Return:** hoàn tồn kho -> set `ReturnedAt` -> `Returned` -> tính phạt `5.000đ / ngày`.
- **Disable:** sách/độc giả được đánh dấu `IsActive=false`, tránh mất lịch sử mượn.

## Thuyết trình PRN212
Các điểm có thể trình bày: Entity relationships, EF Core mapping, LINQ queries, MVVM binding, command pattern, CRUD, validation, transaction-like stock updates, soft delete, SQL Server integration và separation of concerns.

> Lưu ý: môi trường hiện tại không có .NET SDK nên project đã được dựng ở mức source/solution hoàn chỉnh nhưng chưa thể chạy compile trực tiếp trong container này. Khi mở bằng Visual Studio 2022, NuGet sẽ restore các package EF Core cần thiết.
