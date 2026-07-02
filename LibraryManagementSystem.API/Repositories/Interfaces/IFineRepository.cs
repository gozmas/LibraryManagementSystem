using LibraryManagementSystem.API.Models;

namespace LibraryManagementSystem.API.Repositories.Interfaces;

public interface IFineRepository
{
    Task AddAsync(Fine fine);
    Task<IEnumerable<Fine>> GetAllAsync();
    Task<Fine?> GetByIdAsync(int id);
    Task UpdateAsync(Fine fine);
}