using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(LibraryDbContextFactory factory)
    {
        await using var db = factory.CreateDbContext();
        await db.Database.EnsureCreatedAsync();

        if (await db.Categories.AnyAsync()) return;

        var categories = new[]
        {
            new Category { Name = "Công nghệ", Description = "Programming, software, computer science" },
            new Category { Name = "Kinh tế", Description = "Business, finance and management" },
            new Category { Name = "Văn học", Description = "Novels, short stories and literature" },
            new Category { Name = "Khoa học", Description = "Science and research" },
            new Category { Name = "Kỹ năng sống", Description = "Personal growth and soft skills" }
        };
        db.Categories.AddRange(categories);
        await db.SaveChangesAsync();

        var tech = categories[0].CategoryId;
        var business = categories[1].CategoryId;
        var lit = categories[2].CategoryId;
        var science = categories[3].CategoryId;
        var skills = categories[4].CategoryId;

        db.Books.AddRange(
            new Book { ISBN="9780135957059", Title="The Pragmatic Programmer", Author="David Thomas", Publisher="Addison-Wesley", PublishYear=2019, Quantity=8, AvailableQuantity=8, Price=39.99m, ShelfLocation="A-01", CategoryId=tech },
            new Book { ISBN="9780134685991", Title="Effective Java", Author="Joshua Bloch", Publisher="Addison-Wesley", PublishYear=2018, Quantity=6, AvailableQuantity=5, Price=49.90m, ShelfLocation="A-02", CategoryId=tech },
            new Book { ISBN="9781491950357", Title="Designing Data-Intensive Applications", Author="Martin Kleppmann", Publisher="O'Reilly", PublishYear=2017, Quantity=5, AvailableQuantity=4, Price=59.00m, ShelfLocation="A-03", CategoryId=tech },
            new Book { ISBN="9780134494166", Title="Clean Architecture", Author="Robert C. Martin", Publisher="Prentice Hall", PublishYear=2017, Quantity=7, AvailableQuantity=7, Price=55.00m, ShelfLocation="A-04", CategoryId=tech },
            new Book { ISBN="9780062316110", Title="Sapiens", Author="Yuval Noah Harari", Publisher="Harper", PublishYear=2015, Quantity=10, AvailableQuantity=10, Price=24.90m, ShelfLocation="B-01", CategoryId=science },
            new Book { ISBN="9780316346627", Title="Atomic Habits", Author="James Clear", Publisher="Avery", PublishYear=2018, Quantity=9, AvailableQuantity=9, Price=20.00m, ShelfLocation="C-01", CategoryId=skills },
            new Book { ISBN="9780131103627", Title="The C Programming Language", Author="Brian Kernighan", Publisher="Prentice Hall", PublishYear=1988, Quantity=4, AvailableQuantity=4, Price=32.50m, ShelfLocation="A-05", CategoryId=tech },
            new Book { ISBN="9780061120084", Title="To Kill a Mockingbird", Author="Harper Lee", Publisher="Harper Perennial", PublishYear=2006, Quantity=6, AvailableQuantity=6, Price=16.50m, ShelfLocation="D-01", CategoryId=lit },
            new Book { ISBN="9780307474278", Title="The Lean Startup", Author="Eric Ries", Publisher="Crown Business", PublishYear=2011, Quantity=5, AvailableQuantity=5, Price=22.90m, ShelfLocation="B-03", CategoryId=business }
        );
        db.Readers.AddRange(
            new Reader { CardNumber="DG-0001", FullName="Nguyễn Minh Anh", DateOfBirth=new DateTime(2004,5,12), Phone="0901001001", Email="minhanh@example.com", Address="TP. Hồ Chí Minh" },
            new Reader { CardNumber="DG-0002", FullName="Trần Hoàng Nam", DateOfBirth=new DateTime(2003,9,3), Phone="0902002002", Email="hoangnam@example.com", Address="Bình Dương" },
            new Reader { CardNumber="DG-0003", FullName="Lê Gia Hân", DateOfBirth=new DateTime(2005,2,21), Phone="0903003003", Email="giahan@example.com", Address="Đồng Nai" },
            new Reader { CardNumber="DG-0004", FullName="Phạm Quốc Bảo", DateOfBirth=new DateTime(2002,11,8), Phone="0904004004", Email="quocbao@example.com", Address="TP. Hồ Chí Minh" }
        );
        await db.SaveChangesAsync();

        var firstBook = await db.Books.FirstAsync(x => x.ISBN == "9780134685991");
        var secondBook = await db.Books.FirstAsync(x => x.ISBN == "9781491950357");
        var reader1 = await db.Readers.FirstAsync(x => x.CardNumber == "DG-0001");
        var reader2 = await db.Readers.FirstAsync(x => x.CardNumber == "DG-0002");

        var loan1 = new Loan { LoanCode="LN-2026-0001", ReaderId=reader1.ReaderId, BorrowedAt=DateTime.Now.AddDays(-3), DueDate=DateTime.Today.AddDays(11), Status=LoanStatus.Borrowed };
        loan1.Details.Add(new LoanDetail { BookId=firstBook.BookId, Quantity=1 });
        firstBook.AvailableQuantity--;
        db.Loans.Add(loan1);

        var loan2 = new Loan { LoanCode="LN-2026-0002", ReaderId=reader2.ReaderId, BorrowedAt=DateTime.Now.AddDays(-18), DueDate=DateTime.Today.AddDays(-4), Status=LoanStatus.Overdue, FineAmount=20000m };
        loan2.Details.Add(new LoanDetail { BookId=secondBook.BookId, Quantity=1, FineAmount=20000m });
        secondBook.AvailableQuantity--;
        db.Loans.Add(loan2);

        await db.SaveChangesAsync();
    }
}
