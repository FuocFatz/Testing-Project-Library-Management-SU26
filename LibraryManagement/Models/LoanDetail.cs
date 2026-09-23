namespace LibraryManagement.Models;

public class LoanDetail
{
    public int LoanDetailId { get; set; }
    public int LoanId { get; set; }
    public Loan? Loan { get; set; }
    public int BookId { get; set; }
    public Book? Book { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal FineAmount { get; set; }
}
