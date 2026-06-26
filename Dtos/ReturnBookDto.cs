using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.API.Dtos;

public class ReturnBookDto
{
    [Range(1, int.MaxValue)]
    public int LoanId { get; set; }
}