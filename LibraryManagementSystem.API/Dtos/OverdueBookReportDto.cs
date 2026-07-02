namespace LibraryManagementSystem.API.Dtos;

public class OverdueBookReportDto
{
    public int LoanId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int DaysLate { get; set; }
}