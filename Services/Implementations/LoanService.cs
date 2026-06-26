using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using LibraryManagementSystem.API.Services.Interfaces;

namespace LibraryManagementSystem.API.Services.Implementations;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IFineService _fineService;

public LoanService(
    ILoanRepository loanRepository,
    IBookRepository bookRepository,
    IFineService fineService)
{
    _loanRepository = loanRepository;
    _bookRepository = bookRepository;
    _fineService = fineService;
}

    public async Task<IEnumerable<LoanDto>> GetAllAsync()
    {
        var loans = await _loanRepository.GetAllAsync();

        return loans.Select(l => new LoanDto
        {
            Id = l.Id,
            BookId = l.BookId,
            BookTitle = l.Book.Title,
            MemberId = l.MemberId,
            MemberName = l.Member.FirstName + " " + l.Member.LastName,
            BorrowDate = l.BorrowDate,
            DueDate = l.DueDate,
            ReturnDate = l.ReturnDate,
            IsReturned = l.IsReturned
        });
    }

    public async Task<LoanDto?> BorrowBookAsync(BorrowBookDto dto)
    {
        var book = await _bookRepository.GetByIdAsync(dto.BookId);

        if (book == null)
            return null;

        if (!book.IsAvailable)
            return null;

        var loan = new Loan
        {
            BookId = dto.BookId,
            MemberId = dto.MemberId,
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

        return new LoanDto
        {
            Id = createdLoan.Id,
            BookId = createdLoan.BookId,
            BookTitle = createdLoan.Book.Title,
            MemberId = createdLoan.MemberId,
            MemberName = createdLoan.Member.FirstName + " " + createdLoan.Member.LastName,
            BorrowDate = createdLoan.BorrowDate,
            DueDate = createdLoan.DueDate,
            ReturnDate = createdLoan.ReturnDate,
            IsReturned = createdLoan.IsReturned
        };
    }

    public async Task<LoanDto?> ReturnBookAsync(ReturnBookDto dto)
    {
        var loan = await _loanRepository.GetByIdAsync(dto.LoanId);

        if (loan == null)
            return null;

        if (loan.IsReturned)
            return null;

        loan.ReturnDate = DateTime.UtcNow;
        loan.IsReturned = true;

        loan.Book.IsAvailable = true;
        await _fineService.CreateFineIfNeededAsync(loan);

        await _loanRepository.UpdateAsync(loan);

        return new LoanDto
        {
            Id = loan.Id,
            BookId = loan.BookId,
            BookTitle = loan.Book.Title,
            MemberId = loan.MemberId,
            MemberName = loan.Member.FirstName + " " + loan.Member.LastName,
            BorrowDate = loan.BorrowDate,
            DueDate = loan.DueDate,
            ReturnDate = loan.ReturnDate,
            IsReturned = loan.IsReturned
        };
    }
}