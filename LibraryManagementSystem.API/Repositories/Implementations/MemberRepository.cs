using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Repositories.Implementations;

public class MemberRepository : IMemberRepository
{
    private readonly AppDbContext _context;

    public MemberRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Member>> GetAllAsync()
    {
        return await _context.Members.ToListAsync();
    }

    public async Task<Member?> GetByIdAsync(int id)
    {
        return await _context.Members.FindAsync(id);
    }

    public async Task<Member?> GetByUserIdAsync(int userId)
    {
        return await _context.Members
            .FirstOrDefaultAsync(m => m.UserId == userId);
    }

    public async Task AddAsync(Member member)
    {
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Member member)
    {
        _context.Members.Update(member);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Member member)
    {
        _context.Members.Remove(member);
        await _context.SaveChangesAsync();
    }
}