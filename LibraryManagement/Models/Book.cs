namespace LibraryManagement.Models;

public class Book
{
    public int BookId { get; set; }
    public string ISBN { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public int PublishYear { get; set; }
    public int Quantity { get; set; }
    public int AvailableQuantity { get; set; }
    public decimal Price { get; set; }
    public string ShelfLocation { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public ICollection<LoanDetail> LoanDetails { get; set; } = new List<LoanDetail>();
}
