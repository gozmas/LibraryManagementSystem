using LibraryManagementSystem.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Controllers;

[Route("api/dashboard")]
[ApiController]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var totalBooks = await _context.Books.CountAsync();
        var availableBooks = await _context.Books.CountAsync(b => b.IsAvailable);
        var borrowedBooks = await _context.Books.CountAsync(b => !b.IsAvailable);

        var totalAuthors = await _context.Authors.CountAsync();
        var totalCategories = await _context.Categories.CountAsync();
        var totalMembers = await _context.Members.CountAsync();

        var totalLoans = await _context.Loans.CountAsync();
        var activeLoans = await _context.Loans.CountAsync(l => l.ReturnDate == null);
        var returnedLoans = await _context.Loans.CountAsync(l => l.ReturnDate != null);

        var totalFines = await _context.Fines.CountAsync();
        var unpaidFines = await _context.Fines.CountAsync(f => !f.IsPaid);
        var paidFines = await _context.Fines.CountAsync(f => f.IsPaid);
        var totalFineAmount = await _context.Fines.SumAsync(f => f.Amount);

        return Ok(new
        {
            totalBooks,
            availableBooks,
            borrowedBooks,
            totalAuthors,
            totalCategories,
            totalMembers,
            totalLoans,
            activeLoans,
            returnedLoans,
            totalFines,
            unpaidFines,
            paidFines,
            totalFineAmount
        });
    }
}