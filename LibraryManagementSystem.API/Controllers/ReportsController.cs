using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Controllers;

[Route("api/reports")]
[ApiController]
[Authorize(Roles = "Admin")]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        AppDbContext context,
        ILogger<ReportsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("borrowed-books")]
    public async Task<IActionResult> GetBorrowedBooksReport()
    {
        var borrowedBooks = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Where(l => l.ReturnDate == null)
            .Select(l => new BorrowedBookReportDto
            {
                LoanId = l.Id,
                BookTitle = l.Book.Title,
                MemberName = l.Member.FirstName + " " + l.Member.LastName,
                BorrowDate = l.BorrowDate,
                DueDate = l.DueDate
            })
            .ToListAsync();

        _logger.LogInformation("Borrowed books report generated successfully.");

        return Ok(new ApiResponse<IEnumerable<BorrowedBookReportDto>>(
            true,
            "Borrowed books report generated successfully.",
            borrowedBooks));
    }

    [HttpGet("overdue-books")]
    public async Task<IActionResult> GetOverdueBooksReport()
    {
        var overdueBooks = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Where(l =>
                l.ReturnDate == null &&
                l.DueDate < DateTime.UtcNow)
            .Select(l => new OverdueBookReportDto
            {
                LoanId = l.Id,
                BookTitle = l.Book.Title,
                MemberName = l.Member.FirstName + " " + l.Member.LastName,
                DueDate = l.DueDate,
                DaysLate = (DateTime.UtcNow - l.DueDate).Days
            })
            .ToListAsync();

        _logger.LogInformation("Overdue books report generated successfully.");

        return Ok(new ApiResponse<IEnumerable<OverdueBookReportDto>>(
            true,
            "Overdue books report generated successfully.",
            overdueBooks));
    }

    [HttpGet("fines")]
    public async Task<IActionResult> GetFineReports()
    {
        var fines = await _context.Fines
            .Include(f => f.Loan)
                .ThenInclude(l => l.Book)
            .Include(f => f.Loan)
                .ThenInclude(l => l.Member)
            .Select(f => new FineReportDto
            {
                FineId = f.Id,
                MemberName = f.Loan.Member.FirstName + " " + f.Loan.Member.LastName,
                BookTitle = f.Loan.Book.Title,
                Amount = f.Amount,
                IsPaid = f.IsPaid
            })
            .ToListAsync();

        _logger.LogInformation("Fine report generated successfully.");

        return Ok(new ApiResponse<IEnumerable<FineReportDto>>(
            true,
            "Fine report generated successfully.",
            fines));
    }
}