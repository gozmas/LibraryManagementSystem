using System.Text.Json.Serialization;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.API.Models;

public class Member
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int? UserId { get; set; }

    [JsonIgnore]
    public User? User { get; set; }

    [JsonIgnore]
    public ICollection<Loan> Loans { get; set; } = new List<Loan>();
}