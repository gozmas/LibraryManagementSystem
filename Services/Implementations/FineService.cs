using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using LibraryManagementSystem.API.Services.Interfaces;


namespace LibraryManagementSystem.API.Services.Implementations;

public class FineService : IFineService
{
    private readonly IFineRepository _fineRepository;

    private const decimal DailyFineAmount = 5;

    public FineService(IFineRepository fineRepository)
    {
        _fineRepository = fineRepository;
    }

    public async Task CreateFineIfNeededAsync(Loan loan)
    {
        if (loan.ReturnDate == null)
            return;

        if (loan.ReturnDate <= loan.DueDate)
            return;

        var lateBusinessDays = CalculateBusinessDays(loan.DueDate, loan.ReturnDate.Value);

        if (lateBusinessDays <= 0)
            return;

        var fine = new Fine
        {
            LoanId = loan.Id,
            Amount = lateBusinessDays * DailyFineAmount,
            IsPaid = false
        };

        await _fineRepository.AddAsync(fine);
    }
    public async Task<IEnumerable<FineDto>> GetAllAsync()
{
    var fines = await _fineRepository.GetAllAsync();

    return fines.Select(f => new FineDto
    {
        Id = f.Id,
        LoanId = f.LoanId,
        Amount = f.Amount,
        IsPaid = f.IsPaid,
        BookTitle = f.Loan.Book.Title,
        MemberName = f.Loan.Member.FirstName + " " + f.Loan.Member.LastName
    });
}

public async Task<FineDto?> GetByIdAsync(int id)
{
    var fine = await _fineRepository.GetByIdAsync(id);

    if (fine == null)
        return null;

    return new FineDto
    {
        Id = fine.Id,
        LoanId = fine.LoanId,
        Amount = fine.Amount,
        IsPaid = fine.IsPaid,
        BookTitle = fine.Loan.Book.Title,
        MemberName = fine.Loan.Member.FirstName + " " + fine.Loan.Member.LastName
    };
}

public async Task<bool> PayFineAsync(int id)
{
    var fine = await _fineRepository.GetByIdAsync(id);

    if (fine == null)
        return false;

    if (fine.IsPaid)
        return false;

    fine.IsPaid = true;

    await _fineRepository.UpdateAsync(fine);

    return true;
}

    private int CalculateBusinessDays(DateTime dueDate, DateTime returnDate)
    {
        var count = 0;
        var currentDate = dueDate.Date.AddDays(1);
        var endDate = returnDate.Date;

        var holidays = GetOfficialHolidays(returnDate.Year);

        while (currentDate <= endDate)
        {
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