using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Data;

public class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Reader> Readers => Set<Reader>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<LoanDetail> LoanDetails => Set<LoanDetail>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.CategoryId);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(x => x.BookId);
            entity.Property(x => x.ISBN).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Author).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Publisher).HasMaxLength(160);
            entity.Property(x => x.ShelfLocation).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
            entity.HasIndex(x => x.ISBN).IsUnique();
            entity.HasIndex(x => new { x.Title, x.Author });
            entity.HasOne(x => x.Category)
                .WithMany(x => x.Books)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Reader>(entity =>
        {
            entity.HasKey(x => x.ReaderId);
            entity.Property(x => x.CardNumber).HasMaxLength(30).IsRequired();
            entity.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.Address).HasMaxLength(300);
            entity.HasIndex(x => x.CardNumber).IsUnique();
        });

        modelBuilder.Entity<Loan>(entity =>
        {
            entity.HasKey(x => x.LoanId);
            entity.Property(x => x.LoanCode).HasMaxLength(30).IsRequired();
            entity.HasIndex(x => x.LoanCode).IsUnique();
            entity.Property(x => x.FineAmount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Note).HasMaxLength(500);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(x => x.Reader)
                .WithMany(x => x.Loans)
                .HasForeignKey(x => x.ReaderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LoanDetail>(entity =>
        {
            entity.HasKey(x => x.LoanDetailId);
            entity.Property(x => x.FineAmount).HasColumnType("decimal(18,2)");
            entity.HasOne(x => x.Loan)
                .WithMany(x => x.Details)
                .HasForeignKey(x => x.LoanId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Book)
                .WithMany(x => x.LoanDetails)
                .HasForeignKey(x => x.BookId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

public class LibraryDbContextFactory
{
    private readonly string _connectionString;
    public LibraryDbContextFactory(string connectionString) => _connectionString = connectionString;

    public LibraryDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseSqlServer(_connectionString, sql => sql.EnableRetryOnFailure(3))
            .Options;
        return new LibraryDbContext(options);
    }
}
