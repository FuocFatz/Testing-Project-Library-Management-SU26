namespace LibraryManagement.Models;

public enum LoanStatus
{
    Borrowed,
    Returned,
    Overdue,
    Lost
}

public class Loan
{
    public int LoanId { get; set; }
    public string LoanCode { get; set; } = string.Empty;
    public int ReaderId { get; set; }
    public Reader? Reader { get; set; }
    public DateTime BorrowedAt { get; set; } = DateTime.Now;
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);
    public DateTime? ReturnedAt { get; set; }
    public LoanStatus Status { get; set; } = LoanStatus.Borrowed;
    public decimal FineAmount { get; set; }
    public string? Note { get; set; }

    public ICollection<LoanDetail> Details { get; set; } = new List<LoanDetail>();
}
