using System.Text.Json.Serialization;
namespace LibraryManagementSystem.API.Models;

public class Author
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Biography { get; set; }

    [JsonIgnore]
    public ICollection<Book> Books { get; set; } = new List<Book>();
}