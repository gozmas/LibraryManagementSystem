using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Repositories.Implementations;

public class FineRepository : IFineRepository
{
    private readonly AppDbContext _context;

    public FineRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Fine fine)
    {
        await _context.Fines.AddAsync(fine);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Fine>> GetAllAsync()
{
    return await _context.Fines
        .Include(f => f.Loan)
            .ThenInclude(l => l.Book)
        .Include(f => f.Loan)
            .ThenInclude(l => l.Member)
        .ToListAsync();
}
    public async Task<Fine?> GetByIdAsync(int id)
{
    return await _context.Fines
        .Include(f => f.Loan)
        .ThenInclude(l => l.Book)
        .Include(f => f.Loan)
        .ThenInclude(l => l.Member)
        .FirstOrDefaultAsync(f => f.Id == id);
}

public async Task UpdateAsync(Fine fine)
{
    _context.Fines.Update(fine);
    await _context.SaveChangesAsync();
}
}