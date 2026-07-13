using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Repositories.Implementations;

public class WishlistRepository : IWishlistRepository
{
    private readonly AppDbContext _context;

    public WishlistRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Wishlist>> GetByMemberIdAsync(int memberId)
    {
        return await _context.Wishlists
            .Include(w => w.Book)
                .ThenInclude(b => b.Author)
            .Where(w => w.MemberId == memberId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();
    }

    public async Task<Wishlist?> GetByMemberAndBookAsync(int memberId, int bookId)
    {
        return await _context.Wishlists
            .FirstOrDefaultAsync(w => w.MemberId == memberId && w.BookId == bookId);
    }

    public async Task AddAsync(Wishlist wishlist)
    {
        await _context.Wishlists.AddAsync(wishlist);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Wishlist wishlist)
    {
        _context.Wishlists.Remove(wishlist);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Member>> GetMembersWishlistingBookAsync(int bookId)
    {
        return await _context.Wishlists
            .Where(w => w.BookId == bookId)
            .Include(w => w.Member)
            .Select(w => w.Member)
            .Where(m => m.UserId != null)
            .ToListAsync();
    }
}