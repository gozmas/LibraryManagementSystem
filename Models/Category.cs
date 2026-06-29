using System.Text.Json.Serialization;

namespace LibraryManagementSystem.API.Models;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<Book> Books { get; set; } = new List<Book>();
}