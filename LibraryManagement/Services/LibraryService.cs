using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Services;

public sealed class LibraryService
{
    private readonly LibraryDbContextFactory _factory;
    public LibraryService(LibraryDbContextFactory factory) => _factory = factory;

    public async Task<List<Book>> GetBooksAsync(string? search = null)
    {
        await using var db = _factory.CreateDbContext();
        var query = db.Books.Include(x => x.Category).Where(x => x.IsActive).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x => x.Title.Contains(search) || x.Author.Contains(search) || x.ISBN.Contains(search));
        }
        return await query.OrderBy(x => x.Title).ToListAsync();
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        await using var db = _factory.CreateDbContext();
        return await db.Categories.Where(x => x.IsActive).OrderBy(x => x.Name).ToListAsync();
    }

    public async Task<List<Reader>> GetReadersAsync(string? search = null)
    {
        await using var db = _factory.CreateDbContext();
        var query = db.Readers.Where(x => x.IsActive).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(x => x.FullName.Contains(search) || x.CardNumber.Contains(search) || (x.Phone ?? "").Contains(search));
        }
        return await query.OrderBy(x => x.FullName).ToListAsync();
    }

    public async Task<List<Loan>> GetLoansAsync(bool activeOnly = false)
    {
        await using var db = _factory.CreateDbContext();
        var query = db.Loans.Include(x => x.Reader).Include(x => x.Details).ThenInclude(x => x.Book).AsQueryable();
        if (activeOnly) query = query.Where(x => x.Status != LoanStatus.Returned);
        return await query.OrderByDescending(x => x.BorrowedAt).ToListAsync();
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        await using var db = _factory.CreateDbContext();
        var totalBooks = await db.Books.Where(x => x.IsActive).SumAsync(x => x.Quantity);
        var availableBooks = await db.Books.Where(x => x.IsActive).SumAsync(x => x.AvailableQuantity);
        var totalReaders = await db.Readers.CountAsync(x => x.IsActive);
        var activeLoans = await db.Loans.CountAsync(x => x.Status != LoanStatus.Returned);
        var overdue = await db.Loans.CountAsync(x => x.Status == LoanStatus.Overdue || (x.Status == LoanStatus.Borrowed && x.DueDate < DateTime.Today));
        var fines = await db.Loans.SumAsync(x => x.FineAmount);
        var topBooks = await db.LoanDetails
            .GroupBy(x => new { x.BookId, x.Book!.Title })
            .Select(g => new TopBook(g.Key.Title, g.Sum(x => x.Quantity)))
            .OrderByDescending(x => x.BorrowCount)
            .Take(5)
            .ToListAsync();
        return new DashboardStats(totalBooks, availableBooks, totalReaders, activeLoans, overdue, fines, topBooks);
    }

    public async Task SaveBookAsync(Book input, int? editingId)
    {
        await using var db = _factory.CreateDbContext();
        if (editingId is null)
        {
            if (await db.Books.AnyAsync(x => x.ISBN == input.ISBN)) throw new InvalidOperationException("ISBN đã tồn tại.");
            db.Books.Add(input);
        }
        else
        {
            var entity = await db.Books.FirstAsync(x => x.BookId == editingId.Value);
            var delta = input.Quantity - entity.Quantity;
            entity.ISBN = input.ISBN;
            entity.Title = input.Title;
            entity.Author = input.Author;
            entity.Publisher = input.Publisher;
            entity.PublishYear = input.PublishYear;
            entity.Price = input.Price;
            entity.ShelfLocation = input.ShelfLocation;
            entity.CategoryId = input.CategoryId;
            entity.Quantity = input.Quantity;
            entity.AvailableQuantity = Math.Max(0, Math.Min(input.Quantity, entity.AvailableQuantity + delta));
        }
        await db.SaveChangesAsync();
    }

    public async Task DeleteBookAsync(int bookId)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.Books.FirstAsync(x => x.BookId == bookId);
        entity.IsActive = false;
        await db.SaveChangesAsync();
    }

    public async Task SaveReaderAsync(Reader input, int? editingId)
    {
        await using var db = _factory.CreateDbContext();
        if (editingId is null)
        {
            if (await db.Readers.AnyAsync(x => x.CardNumber == input.CardNumber)) throw new InvalidOperationException("Mã thẻ đã tồn tại.");
            db.Readers.Add(input);
        }
        else
        {
            var entity = await db.Readers.FirstAsync(x => x.ReaderId == editingId.Value);
            entity.CardNumber = input.CardNumber;
            entity.FullName = input.FullName;
            entity.DateOfBirth = input.DateOfBirth;
            entity.Phone = input.Phone;
            entity.Email = input.Email;
            entity.Address = input.Address;
        }
        await db.SaveChangesAsync();
    }

    public async Task DeleteReaderAsync(int readerId)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.Readers.FirstAsync(x => x.ReaderId == readerId);
        var hasActiveLoans = await db.Loans.AnyAsync(x => x.ReaderId == readerId && x.Status != LoanStatus.Returned);
        if (hasActiveLoans) throw new InvalidOperationException("Độc giả còn phiếu mượn chưa trả, không thể vô hiệu hóa.");
        entity.IsActive = false;
        await db.SaveChangesAsync();
    }

    public async Task BorrowAsync(int readerId, int bookId, DateTime dueDate, string? note)
    {
        await using var db = _factory.CreateDbContext();
        var reader = await db.Readers.FirstOrDefaultAsync(x => x.ReaderId == readerId && x.IsActive);
        var book = await db.Books.FirstOrDefaultAsync(x => x.BookId == bookId && x.IsActive);
        if (reader is null) throw new InvalidOperationException("Không tìm thấy độc giả.");
        if (book is null) throw new InvalidOperationException("Không tìm thấy sách.");
        if (book.AvailableQuantity <= 0) throw new InvalidOperationException("Sách này hiện đã hết bản có thể mượn.");
        if (dueDate.Date < DateTime.Today) throw new InvalidOperationException("Ngày trả dự kiến không hợp lệ.");

        var loan = new Loan
        {
            LoanCode = $"LN-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
            ReaderId = readerId,
            BorrowedAt = DateTime.Now,
            DueDate = dueDate,
            Note = note,
            Status = LoanStatus.Borrowed,
            Details = new List<LoanDetail> { new() { BookId = bookId, Quantity = 1 } }
        };
        book.AvailableQuantity--;
        db.Loans.Add(loan);
        await db.SaveChangesAsync();
    }

    public async Task ReturnAsync(int loanId)
    {
        await using var db = _factory.CreateDbContext();
        var loan = await db.Loans.Include(x => x.Details).FirstOrDefaultAsync(x => x.LoanId == loanId);
        if (loan is null) throw new InvalidOperationException("Không tìm thấy phiếu mượn.");
        if (loan.Status == LoanStatus.Returned) throw new InvalidOperationException("Phiếu này đã trả trước đó.");

        var books = await db.Books.Where(x => loan.Details.Select(d => d.BookId).Contains(x.BookId)).ToDictionaryAsync(x => x.BookId);
        foreach (var detail in loan.Details)
            if (books.TryGetValue(detail.BookId, out var book)) book.AvailableQuantity = Math.Min(book.Quantity, book.AvailableQuantity + detail.Quantity);

        loan.ReturnedAt = DateTime.Now;
        loan.Status = LoanStatus.Returned;
        loan.FineAmount = CalculateFine(loan.DueDate, loan.ReturnedAt.Value);
        await db.SaveChangesAsync();
    }

    public async Task UpdateOverdueStatusesAsync()
    {
        await using var db = _factory.CreateDbContext();
        var overdue = await db.Loans.Where(x => x.Status == LoanStatus.Borrowed && x.DueDate < DateTime.Today).ToListAsync();
        foreach (var loan in overdue) loan.Status = LoanStatus.Overdue;
        await db.SaveChangesAsync();
    }

    private static decimal CalculateFine(DateTime dueDate, DateTime returnedAt)
    {
        var days = Math.Max(0, (returnedAt.Date - dueDate.Date).Days);
        return days * 5000m;
    }
}

public record DashboardStats(int TotalBooks, int AvailableBooks, int TotalReaders, int ActiveLoans, int OverdueLoans, decimal TotalFines, List<TopBook> TopBooks);
public record TopBook(string Title, int BorrowCount);
