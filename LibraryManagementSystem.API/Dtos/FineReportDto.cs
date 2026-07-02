namespace LibraryManagementSystem.API.Dtos;

public class FineReportDto
{
    public int FineId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string BookTitle { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
}