using LibraryManagementSystem.API.Models;

namespace LibraryManagementSystem.API.Services.Interfaces;

public interface IMemberService
{
    Task<IEnumerable<Member>> GetAllAsync();
    Task<Member?> GetByIdAsync(int id);
    Task<Member?> GetByUserIdAsync(int userId);
    Task AddAsync(Member member);
    Task UpdateAsync(Member member);
    Task DeleteAsync(Member member);
}