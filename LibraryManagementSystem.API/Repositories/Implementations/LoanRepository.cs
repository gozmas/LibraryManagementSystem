using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Repositories.Implementations;

public class LoanRepository : ILoanRepository
{
    private readonly AppDbContext _context;

    public LoanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Loan>> GetAllAsync()
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .ToListAsync();
    }

    public async Task<Loan?> GetByIdAsync(int id)
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task AddAsync(Loan loan)
    {
        await _context.Loans.AddAsync(loan);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Loan loan)
    {
        _context.Loans.Update(loan);
        await _context.SaveChangesAsync();
    }
}