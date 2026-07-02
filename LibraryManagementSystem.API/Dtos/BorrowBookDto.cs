using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.API.Dtos;

public class BorrowBookDto
{
    [Range(1, int.MaxValue)]
    public int BookId { get; set; }

    [Range(1, int.MaxValue)]
    public int MemberId { get; set; }
}