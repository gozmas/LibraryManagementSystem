namespace LibraryManagementSystem.API.Models;

public enum FineReason
{
    Late,
    Damaged,
    Lost
}

public class Fine
{
    public int Id { get; set; }

    public decimal Amount { get; set; }

    public bool IsPaid { get; set; }

    public FineReason Reason { get; set; } = FineReason.Late;

    public int LoanId { get; set; }

    public Loan Loan { get; set; } = null!;
}