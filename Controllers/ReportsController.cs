using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Controllers;

[Route("api/reports")]
[ApiController]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
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

        return Ok(borrowedBooks);
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

    return Ok(overdueBooks);
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

    return Ok(fines);
}
}