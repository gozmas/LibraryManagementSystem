using System.Text.Json.Serialization;

namespace LibraryManagementSystem.API.Models;

public class BookCopy
{
    public int Id { get; set; }

    public int CopyNumber { get; set; }

    public CopyStatus Status { get; set; } = CopyStatus.Available;

    public string? ConditionNote { get; set; }

    public int BookId { get; set; }
    public Book Book { get; set; } = null!;

    [JsonIgnore]
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}