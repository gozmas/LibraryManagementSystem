using System.Text.Json.Serialization;

namespace LibraryManagementSystem.API.Models;

public class Wishlist
{
    public int Id { get; set; }

    public int MemberId { get; set; }

    [JsonIgnore]
    public Member Member { get; set; } = null!;

    public int BookId { get; set; }

    public Book Book { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}