using LibraryManagementSystem.Models;
namespace LibraryManagementSystem.API.Models;

public class Member
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public ICollection<Loan> Loans { get; set; } = new List<Loan>();

     public int? UserId { get; set; }          // nullable — eski kayıtlar bozulmasın
    public User? User { get; set; }

   
}