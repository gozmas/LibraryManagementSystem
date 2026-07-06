using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.API.Dtos;

public class ReturnBookDto
{
    [Range(1, int.MaxValue)]
    public int LoanId { get; set; }

    public string Condition { get; set; } = "Good";

    [StringLength(500)]
    public string? ConditionNote { get; set; }
}