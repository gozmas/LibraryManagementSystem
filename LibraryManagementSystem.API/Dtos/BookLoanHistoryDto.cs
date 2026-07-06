namespace LibraryManagementSystem.API.Dtos;

public class BookLoanHistoryDto
{
    public int LoanId { get; set; }

    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;

    public int BookId { get; set; }
    public int CopyNumber { get; set; }
    public string CopyStatus { get; set; } = string.Empty;

    public DateTime BorrowDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsReturned { get; set; }
}