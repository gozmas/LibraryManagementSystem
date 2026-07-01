using System.Security.Claims;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Responses;
using LibraryManagementSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.API.Controllers;

[Route("api/loans")]
[ApiController]
public class LoanController : ControllerBase
{
    private readonly ILoanService _loanService;
    private readonly ILogger<LoanController> _logger;

    public LoanController(
        ILoanService loanService,
        ILogger<LoanController> logger)
    {
        _loanService = loanService;
        _logger = logger;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetLoans()
    {
        var loans = await _loanService.GetAllAsync();

        _logger.LogInformation("Loans listed successfully.");

        return Ok(new ApiResponse<IEnumerable<LoanDto>>(
            true,
            "Loans retrieved successfully.",
            loans));
    }

    [Authorize(Roles = "Admin,Member,Student")]
    [HttpPost("borrow")]
    public async Task<IActionResult> BorrowBook(BorrowBookDto dto)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new ApiResponse<object>(
                false,
                "User information could not be found.",
                null));
        }

        var isAdmin = User.IsInRole("Admin");

        var result = await _loanService.BorrowBookAsync(dto, userId.Value, isAdmin);

        if (result == null)
        {
            _logger.LogWarning(
                "Borrow failed. BookId: {BookId}, RequestedMemberId: {MemberId}, UserId: {UserId}",
                dto.BookId,
                dto.MemberId,
                userId.Value);

            return BadRequest(new ApiResponse<object>(
                false,
                "Book not found, not available, or member not found.",
                null));
        }

        _logger.LogInformation(
            "Book borrowed successfully. BookId: {BookId}, UserId: {UserId}",
            dto.BookId,
            userId.Value);

        return Ok(new ApiResponse<LoanDto>(
            true,
            "Book borrowed successfully.",
            result));
    }

    [Authorize(Roles = "Admin,Member,Student")]
    [HttpPost("return")]
    public async Task<IActionResult> ReturnBook(ReturnBookDto dto)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new ApiResponse<object>(
                false,
                "User information could not be found.",
                null));
        }

        var isAdmin = User.IsInRole("Admin");

        var result = await _loanService.ReturnBookAsync(dto, userId.Value, isAdmin);

        if (result == null)
        {
            _logger.LogWarning(
                "Return failed. LoanId: {LoanId}, UserId: {UserId}",
                dto.LoanId,
                userId.Value);

            return BadRequest(new ApiResponse<object>(
                false,
                "Loan not found, already returned, or you are not allowed to return this loan.",
                null));
        }

        _logger.LogInformation(
            "Book returned successfully. LoanId: {LoanId}, UserId: {UserId}",
            dto.LoanId,
            userId.Value);

        return Ok(new ApiResponse<LoanDto>(
            true,
            "Book returned successfully.",
            result));
    }

    [Authorize(Roles = "Member,Student")]
    [HttpGet("my")]
    public async Task<IActionResult> GetMyLoans()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized(new ApiResponse<object>(
                false,
                "User information could not be found.",
                null));
        }

        var loans = await _loanService.GetMyLoansAsync(userId.Value);

        _logger.LogInformation(
            "My loans retrieved successfully. UserId: {UserId}",
            userId.Value);

        return Ok(new ApiResponse<IEnumerable<LoanDto>>(
            true,
            "My loans retrieved successfully.",
            loans));
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
        {
            return null;
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}