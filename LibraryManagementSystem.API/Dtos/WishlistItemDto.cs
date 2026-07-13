namespace LibraryManagementSystem.API.Dtos;

public class WishlistItemDto
{
    public int Id { get; set; }

    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string? CoverUrl { get; set; }

    public bool IsAvailable { get; set; }
    public int AvailableCopies { get; set; }
    public int TotalCopies { get; set; }

    public DateTime CreatedAt { get; set; }
}