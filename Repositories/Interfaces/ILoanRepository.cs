using LibraryManagementSystem.API.Models;

namespace LibraryManagementSystem.API.Repositories.Interfaces;

public interface ILoanRepository
{
    Task<IEnumerable<Loan>> GetAllAsync();
    Task<Loan?> GetByIdAsync(int id);
    Task AddAsync(Loan loan);
    Task UpdateAsync(Loan loan);
}