using LibraryManagementSystem.API.Responses;
using Microsoft.AspNetCore.Authorization;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Services.Interfaces;
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

    [Authorize(Roles = "Admin,Student")]
    [HttpPost("borrow")]
    public async Task<IActionResult> BorrowBook(BorrowBookDto dto)
    {
        var result = await _loanService.BorrowBookAsync(dto);

        if (result == null)
        {
            _logger.LogWarning(
                "Borrow failed. BookId: {BookId}, MemberId: {MemberId}",
                dto.BookId,
                dto.MemberId);

            return BadRequest(new ApiResponse<object>(
                false,
                "Book not found or not available.",
                null));
        }

        _logger.LogInformation(
            "Book borrowed successfully. BookId: {BookId}, MemberId: {MemberId}",
            dto.BookId,
            dto.MemberId);

        return Ok(new ApiResponse<LoanDto>(
            true,
            "Book borrowed successfully.",
            result));
    }

    [Authorize(Roles = "Admin,Student")]
    [HttpPost("return")]
    public async Task<IActionResult> ReturnBook(ReturnBookDto dto)
    {
        var result = await _loanService.ReturnBookAsync(dto);

        if (result == null)
        {
            _logger.LogWarning(
                "Return failed. LoanId: {LoanId}",
                dto.LoanId);

            return BadRequest(new ApiResponse<object>(
                false,
                "Loan not found or already returned.",
                null));
        }

        _logger.LogInformation(
            "Book returned successfully. LoanId: {LoanId}",
            dto.LoanId);

        return Ok(new ApiResponse<LoanDto>(
            true,
            "Book returned successfully.",
            result));
    }
}