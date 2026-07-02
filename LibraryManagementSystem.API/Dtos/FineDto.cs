namespace LibraryManagementSystem.API.Dtos;

public class FineDto
{
    public int Id { get; set; }
    public int LoanId { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }

    public string BookTitle { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
}