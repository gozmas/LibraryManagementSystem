using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Extensions;
using LibraryManagementSystem.API.Responses;
using LibraryManagementSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers;

[Route("api/wishlist")]
[ApiController]
[Authorize(Roles = "Member,Student")]
public class WishlistController : ControllerBase
{
    private readonly IWishlistService _wishlistService;
    private readonly ILogger<WishlistController> _logger;

    public WishlistController(
        IWishlistService wishlistService,
        ILogger<WishlistController> logger)
    {
        _wishlistService = wishlistService;
        _logger = logger;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyWishlist()
    {
        var userId = User.GetUserId();

        if (userId == null)
        {
            return Unauthorized(new ApiResponse<object>(
                false,
                "User information could not be found.",
                null));
        }

        var result = await _wishlistService.GetMyWishlistAsync(userId.Value);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<object>(
                false,
                result.ErrorMessage ?? "Wishlist could not be retrieved.",
                null));
        }

        return Ok(new ApiResponse<IEnumerable<WishlistItemDto>>(
            true,
            "Wishlist retrieved successfully.",
            result.Data));
    }

    [HttpPost]
    public async Task<IActionResult> AddToWishlist(AddToWishlistDto dto)
    {
        var userId = User.GetUserId();

        if (userId == null)
        {
            return Unauthorized(new ApiResponse<object>(
                false,
                "User information could not be found.",
                null));
        }

        var result = await _wishlistService.AddToWishlistAsync(userId.Value, dto.BookId);

        if (!result.Success)
        {
            _logger.LogWarning(
                "Add to wishlist failed. BookId: {BookId}, UserId: {UserId}, Reason: {Reason}",
                dto.BookId,
                userId.Value,
                result.ErrorMessage);

            return StatusCode(result.StatusCode, new ApiResponse<object>(
                false,
                result.ErrorMessage ?? "Book could not be added to wishlist.",
                null));
        }

        return Ok(new ApiResponse<WishlistItemDto>(
            true,
            "Book added to wishlist.",
            result.Data));
    }

    [HttpDelete("{bookId}")]
    public async Task<IActionResult> RemoveFromWishlist(int bookId)
    {
        var userId = User.GetUserId();

        if (userId == null)
        {
            return Unauthorized(new ApiResponse<object>(
                false,
                "User information could not be found.",
                null));
        }

        var result = await _wishlistService.RemoveFromWishlistAsync(userId.Value, bookId);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new ApiResponse<object>(
                false,
                result.ErrorMessage ?? "Book could not be removed from wishlist.",
                null));
        }

        return Ok(new ApiResponse<object>(
            true,
            "Book removed from wishlist.",
            null));
    }
}