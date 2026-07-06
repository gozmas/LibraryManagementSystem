using System.Text.Json.Serialization;
namespace LibraryManagementSystem.API.Models;

public class Book
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ISBN { get; set; } = string.Empty;

    public int PublicationYear { get; set; }

    public bool IsAvailable { get; set; } = true;

    public int TotalCopies { get; set; } = 1;

    public int AvailableCopies { get; set; } = 1;

    public string? Description { get; set; }

    public string? CoverUrl { get; set; }

    public int AuthorId { get; set; }
    public Author Author { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    [JsonIgnore]
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

    [JsonIgnore]
    public ICollection<BookCopy> Copies { get; set; } = new List<BookCopy>();
}