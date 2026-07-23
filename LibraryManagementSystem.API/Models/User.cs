using LibraryManagementSystem.API.Models;
namespace LibraryManagementSystem.Models

{
    public class User
    {
        public int Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "Member";
        public bool IsActive { get; set; } = true;

        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpiry { get; set; }

        public Member? Member { get; set; }
    }
}