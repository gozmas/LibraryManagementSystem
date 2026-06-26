using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Controllers;

[Route("api/dashboard")]
[ApiController]
[Authorize(Roles = "Admin")]
public class DashboardController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        AppDbContext context,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _logger = logger;
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

        var mostBorrowedBook = await _context.Loans
            .GroupBy(l => l.Book.Title)
            .Select(g => new
            {
                BookTitle = g.Key,
                BorrowCount = g.Count()
            })
            .OrderByDescending(x => x.BorrowCount)
            .FirstOrDefaultAsync();

        var mostActiveMember = await _context.Loans
            .GroupBy(l => l.Member.FirstName + " " + l.Member.LastName)
            .Select(g => new
            {
                MemberName = g.Key,
                BorrowCount = g.Count()
            })
            .OrderByDescending(x => x.BorrowCount)
            .FirstOrDefaultAsync();

        var averageFineAmount = await _context.Fines.AnyAsync()
            ? await _context.Fines.AverageAsync(f => f.Amount)
            : 0;

        var booksBorrowedToday = await _context.Loans
            .CountAsync(l => l.BorrowDate.Date == DateTime.UtcNow.Date);

        var statistics = new
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
            totalFineAmount,
            MostBorrowedBook = mostBorrowedBook?.BookTitle,
            MostActiveMember = mostActiveMember?.MemberName,
            AverageFineAmount = averageFineAmount,
            BooksBorrowedToday = booksBorrowedToday
        };

        _logger.LogInformation("Dashboard statistics retrieved successfully.");

        return Ok(new ApiResponse<object>(
            true,
            "Dashboard statistics retrieved successfully.",
            statistics));
    }
}