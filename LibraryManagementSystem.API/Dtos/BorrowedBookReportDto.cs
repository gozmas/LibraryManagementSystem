namespace LibraryManagementSystem.API.Dtos;

public class BorrowedBookReportDto
{
    public int LoanId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
}