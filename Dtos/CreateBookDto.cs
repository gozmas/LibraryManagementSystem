using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.API.Dtos;

public class CreateBookDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string ISBN { get; set; } = string.Empty;

    [Range(1000, 2100)]
    public int PublicationYear { get; set; }

    [Range(1, int.MaxValue)]
    public int AuthorId { get; set; }

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}