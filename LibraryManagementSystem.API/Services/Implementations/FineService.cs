using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using LibraryManagementSystem.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Services.Implementations;

public class FineService : IFineService
{
    private readonly IFineRepository _fineRepository;
    private readonly AppDbContext _context;

    private const decimal DailyFineAmount = 5;
    private const decimal DamagedFineAmount = 50;
    private const decimal LostFineAmount = 150;

    public FineService(
        IFineRepository fineRepository,
        AppDbContext context)
    {
        _fineRepository = fineRepository;
        _context = context;
    }

    public async Task CreateFineIfNeededAsync(Loan loan)
    {
        if (loan.ReturnDate == null)
            return;

        if (loan.ReturnDate <= loan.DueDate)
            return;

        var existingLateFine = await _context.Fines
            .AnyAsync(f => f.LoanId == loan.Id && f.Reason == FineReason.Late);

        if (existingLateFine)
            return;

        var lateBusinessDays = CalculateBusinessDays(
            loan.DueDate,
            loan.ReturnDate.Value);

        if (lateBusinessDays <= 0)
            return;

        var fine = new Fine
        {
            LoanId = loan.Id,
            Amount = lateBusinessDays * DailyFineAmount,
            IsPaid = false,
            Reason = FineReason.Late
        };

        await _fineRepository.AddAsync(fine);
    }

    public async Task CreateConditionFineIfNeededAsync(Loan loan, string condition)
    {
        FineReason? reason = condition switch
        {
            "Damaged" => FineReason.Damaged,
            "Lost" => FineReason.Lost,
            _ => null
        };

        if (reason == null)
            return;

        var existingConditionFine = await _context.Fines
            .AnyAsync(f => f.LoanId == loan.Id && f.Reason == reason);

        if (existingConditionFine)
            return;

        var amount = reason == FineReason.Lost
            ? LostFineAmount
            : DamagedFineAmount;

        var fine = new Fine
        {
            LoanId = loan.Id,
            Amount = amount,
            IsPaid = false,
            Reason = reason.Value
        };

        await _fineRepository.AddAsync(fine);
    }

    public async Task<IEnumerable<FineDto>> GetAllAsync()
    {
        var fines = await _context.Fines
            .Include(f => f.Loan)
                .ThenInclude(l => l.Book)
            .Include(f => f.Loan)
                .ThenInclude(l => l.Member)
            .OrderByDescending(f => f.Id)
            .ToListAsync();

        return fines.Select(MapToFineDto);
    }

    public async Task<FineDto?> GetByIdAsync(int id)
    {
        var fine = await _context.Fines
            .Include(f => f.Loan)
                .ThenInclude(l => l.Book)
            .Include(f => f.Loan)
                .ThenInclude(l => l.Member)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (fine == null)
            return null;

        return MapToFineDto(fine);
    }

    public async Task<IEnumerable<FineDto>> GetMyFinesAsync(int userId)
    {
        var fines = await _context.Fines
            .Include(f => f.Loan)
                .ThenInclude(l => l.Book)
            .Include(f => f.Loan)
                .ThenInclude(l => l.Member)
            .Where(f => f.Loan.Member.UserId == userId)
            .OrderByDescending(f => f.Id)
            .ToListAsync();

        return fines.Select(MapToFineDto);
    }

    public async Task<bool> PayFineAsync(
        int id,
        int userId,
        bool isAdmin)
    {
        var fine = await _context.Fines
            .Include(f => f.Loan)
                .ThenInclude(l => l.Book)
            .Include(f => f.Loan)
                .ThenInclude(l => l.Member)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (fine == null)
            return false;

        if (fine.IsPaid)
            return false;

        if (!isAdmin)
        {
            var currentMember = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (currentMember == null)
                return false;

            if (fine.Loan.MemberId != currentMember.Id)
                return false;
        }

        fine.IsPaid = true;

        await _fineRepository.UpdateAsync(fine);

        return true;
    }

    private static FineDto MapToFineDto(Fine fine)
    {
        return new FineDto
        {
            Id = fine.Id,
            LoanId = fine.LoanId,
            Amount = fine.Amount,
            IsPaid = fine.IsPaid,
            Reason = fine.Reason.ToString(),

            BookTitle = fine.Loan?.Book != null
                ? fine.Loan.Book.Title
                : string.Empty,

            MemberName = fine.Loan?.Member != null
                ? fine.Loan.Member.FirstName + " " + fine.Loan.Member.LastName
                : string.Empty
        };
    }

    private int CalculateBusinessDays(DateTime dueDate, DateTime returnDate)
    {
        var count = 0;
        var currentDate = dueDate.Date.AddDays(1);
        var endDate = returnDate.Date;

        while (currentDate <= endDate)
        {
            var holidays = GetOfficialHolidays(currentDate.Year);

            var isWeekend =
                currentDate.DayOfWeek == DayOfWeek.Saturday ||
                currentDate.DayOfWeek == DayOfWeek.Sunday;

            var isHoliday = holidays.Contains(currentDate);

            if (!isWeekend && !isHoliday)
            {
                count++;
            }

            currentDate = currentDate.AddDays(1);
        }

        return count;
    }

    private List<DateTime> GetOfficialHolidays(int year)
    {
        return new List<DateTime>
        {
            new DateTime(year, 1, 1),
            new DateTime(year, 4, 23),
            new DateTime(year, 5, 1),
            new DateTime(year, 5, 19),
            new DateTime(year, 7, 15),
            new DateTime(year, 8, 30),
            new DateTime(year, 10, 29)
        };
    }
}