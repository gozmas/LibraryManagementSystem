using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using LibraryManagementSystem.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Services.Implementations;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IFineService _fineService;
    private readonly AppDbContext _context;

    public LoanService(
        ILoanRepository loanRepository,
        IBookRepository bookRepository,
        IFineService fineService,
        AppDbContext context)
    {
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
        _fineService = fineService;
        _context = context;
    }

    public async Task<IEnumerable<LoanDto>> GetAllAsync()
    {
        var loans = await _loanRepository.GetAllAsync();

        return loans.Select(MapToLoanDto);
    }

    public async Task<LoanDto?> BorrowBookAsync(
        BorrowBookDto dto,
        int userId,
        bool isAdmin)
    {
        var book = await _bookRepository.GetByIdAsync(dto.BookId);

        if (book == null)
            return null;

        if (!book.IsAvailable)
            return null;

        int memberId;

        if (isAdmin)
        {
            var memberExists = await _context.Members
                .AnyAsync(m => m.Id == dto.MemberId);

            if (!memberExists)
                return null;

            memberId = dto.MemberId;
        }
        else
        {
            var currentMember = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (currentMember == null)
                return null;

            memberId = currentMember.Id;
        }

        var loan = new Loan
        {
            BookId = dto.BookId,
            MemberId = memberId,
            BorrowDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            IsReturned = false
        };

        book.IsAvailable = false;

        await _loanRepository.AddAsync(loan);
        await _bookRepository.UpdateAsync(book);

        var createdLoan = await _loanRepository.GetByIdAsync(loan.Id);

        if (createdLoan == null)
            return null;

        return MapToLoanDto(createdLoan);
    }

    public async Task<LoanDto?> ReturnBookAsync(
        ReturnBookDto dto,
        int userId,
        bool isAdmin)
    {
        var loan = await _loanRepository.GetByIdAsync(dto.LoanId);

        if (loan == null)
            return null;

        if (loan.IsReturned || loan.ReturnDate != null)
            return null;

        if (!isAdmin)
        {
            var currentMember = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (currentMember == null)
                return null;

            if (loan.MemberId != currentMember.Id)
                return null;
        }

        loan.ReturnDate = DateTime.UtcNow;
        loan.IsReturned = true;

        if (loan.Book != null)
        {
            loan.Book.IsAvailable = true;
        }

        await _fineService.CreateFineIfNeededAsync(loan);

        await _loanRepository.UpdateAsync(loan);

        return MapToLoanDto(loan);
    }

    public async Task<IEnumerable<LoanDto>> GetMyLoansAsync(int userId)
    {
        var loans = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Where(l => l.Member.UserId == userId)
            .OrderByDescending(l => l.BorrowDate)
            .ToListAsync();

        return loans.Select(MapToLoanDto);
    }

    private static LoanDto MapToLoanDto(Loan loan)
    {
        return new LoanDto
        {
            Id = loan.Id,
            BookId = loan.BookId,
            BookTitle = loan.Book != null
                ? loan.Book.Title
                : string.Empty,

            MemberId = loan.MemberId,
            MemberName = loan.Member != null
                ? loan.Member.FirstName + " " + loan.Member.LastName
                : string.Empty,

            BorrowDate = loan.BorrowDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            IsReturned = loan.IsReturned
        };
    }
}