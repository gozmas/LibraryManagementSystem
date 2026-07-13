using LibraryManagementSystem.API.Common;
using LibraryManagementSystem.API.Dtos;

namespace LibraryManagementSystem.API.Services.Interfaces;

public interface IWishlistService
{
    Task<ServiceResult<IEnumerable<WishlistItemDto>>> GetMyWishlistAsync(int userId);
    Task<ServiceResult<WishlistItemDto>> AddToWishlistAsync(int userId, int bookId);
    Task<ServiceResult<bool>> RemoveFromWishlistAsync(int userId, int bookId);
}
