using LibraryManagementSystem.API.Common;
using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using LibraryManagementSystem.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Services.Implementations;

public class WishlistService : IWishlistService
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IBookRepository _bookRepository;
    private readonly AppDbContext _context;

    public WishlistService(
        IWishlistRepository wishlistRepository,
        IBookRepository bookRepository,
        AppDbContext context)
    {
        _wishlistRepository = wishlistRepository;
        _bookRepository = bookRepository;
        _context = context;
    }

    public async Task<ServiceResult<IEnumerable<WishlistItemDto>>> GetMyWishlistAsync(int userId)
    {
        var member = await GetMemberByUserIdAsync(userId);

        if (member == null)
            return ServiceResult<IEnumerable<WishlistItemDto>>.Fail(
                "No member profile found for the current user.", 404);

        var items = await _wishlistRepository.GetByMemberIdAsync(member.Id);

        return ServiceResult<IEnumerable<WishlistItemDto>>.Ok(items.Select(MapToDto));
    }

    public async Task<ServiceResult<WishlistItemDto>> AddToWishlistAsync(int userId, int bookId)
    {
        var member = await GetMemberByUserIdAsync(userId);

        if (member == null)
            return ServiceResult<WishlistItemDto>.Fail(
                "No member profile found for the current user.", 404);

        var book = await _bookRepository.GetByIdAsync(bookId);

        if (book == null)
            return ServiceResult<WishlistItemDto>.Fail("Book not found.", 404);

        var existing = await _wishlistRepository.GetByMemberAndBookAsync(member.Id, bookId);

        if (existing != null)
            return ServiceResult<WishlistItemDto>.Fail("This book is already in your wishlist.", 409);

        var wishlist = new Wishlist
        {
            MemberId = member.Id,
            BookId = bookId,
            CreatedAt = DateTime.UtcNow,
        };

        await _wishlistRepository.AddAsync(wishlist);

        wishlist.Book = book;

        return ServiceResult<WishlistItemDto>.Ok(MapToDto(wishlist));
    }

    public async Task<ServiceResult<bool>> RemoveFromWishlistAsync(int userId, int bookId)
    {
        var member = await GetMemberByUserIdAsync(userId);

        if (member == null)
            return ServiceResult<bool>.Fail("No member profile found for the current user.", 404);

        var existing = await _wishlistRepository.GetByMemberAndBookAsync(member.Id, bookId);

        if (existing == null)
            return ServiceResult<bool>.Fail("This book is not in your wishlist.", 404);

        await _wishlistRepository.RemoveAsync(existing);

        return ServiceResult<bool>.Ok(true);
    }

    private async Task<Member?> GetMemberByUserIdAsync(int userId)
    {
        return await _context.Members.FirstOrDefaultAsync(m => m.UserId == userId);
    }

    private static WishlistItemDto MapToDto(Wishlist wishlist)
    {
        return new WishlistItemDto
        {
            Id = wishlist.Id,
            BookId = wishlist.BookId,
            BookTitle = wishlist.Book != null ? wishlist.Book.Title : string.Empty,
            AuthorName = wishlist.Book?.Author != null
                ? wishlist.Book.Author.FirstName + " " + wishlist.Book.Author.LastName
                : string.Empty,
            CoverUrl = wishlist.Book?.CoverUrl,
            IsAvailable = wishlist.Book?.IsAvailable ?? false,
            AvailableCopies = wishlist.Book?.AvailableCopies ?? 0,
            TotalCopies = wishlist.Book?.TotalCopies ?? 0,
            CreatedAt = wishlist.CreatedAt,
        };
    }
}