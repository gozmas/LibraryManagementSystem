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

    public LoanController(ILoanService loanService)
    {
        _loanService = loanService;
    }

   [Authorize(Roles = "Admin")]
[HttpGet]
public async Task<IActionResult> GetLoans()
{
    var loans = await _loanService.GetAllAsync();
    return Ok(loans);
}
     [Authorize(Roles = "Admin,Student")]
    [HttpPost("borrow")]
    public async Task<IActionResult> BorrowBook(BorrowBookDto dto)
    {
        var result = await _loanService.BorrowBookAsync(dto);

        if (result == null)
        {
            return BadRequest("Book not found or not available.");
        }

        return Ok(result);
    }
    [Authorize(Roles = "Admin,Student")]
    [HttpPost("return")]
    public async Task<IActionResult> ReturnBook(ReturnBookDto dto)
    {
        var result = await _loanService.ReturnBookAsync(dto);

        if (result == null)
        {
            return BadRequest("Loan not found or already returned.");
        }

        return Ok(result);
    }
}