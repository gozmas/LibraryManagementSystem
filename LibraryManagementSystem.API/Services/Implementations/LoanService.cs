using LibraryManagementSystem.API.Common;
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

    public async Task<ServiceResult<LoanDto>> BorrowBookAsync(
        BorrowBookDto dto,
        int userId,
        bool isAdmin)
    {
        var book = await _bookRepository.GetByIdAsync(dto.BookId);

        if (book == null)
            return ServiceResult<LoanDto>.Fail("Book not found.", 404);

        if (book.AvailableCopies <= 0)
            return ServiceResult<LoanDto>.Fail("This book has no available copies right now.", 409);

        var availableCopy = await _context.BookCopies
            .FirstOrDefaultAsync(c => c.BookId == dto.BookId && c.Status == CopyStatus.Available);

        if (availableCopy == null)
            return ServiceResult<LoanDto>.Fail("This book has no available copies right now.", 409);

        int memberId;

        if (isAdmin)
        {
            var memberExists = await _context.Members
                .AnyAsync(m => m.Id == dto.MemberId);

            if (!memberExists)
                return ServiceResult<LoanDto>.Fail("Member not found.", 404);

            memberId = dto.MemberId;
        }
        else
        {
            var currentMember = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (currentMember == null)
                return ServiceResult<LoanDto>.Fail("No member profile found for the current user.", 404);

            memberId = currentMember.Id;
        }

        var loan = new Loan
        {
            BookId = dto.BookId,
            BookCopyId = availableCopy.Id,
            MemberId = memberId,
            BorrowDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            IsReturned = false
        };

        availableCopy.Status = CopyStatus.Borrowed;

        book.AvailableCopies -= 1;
        book.IsAvailable = book.AvailableCopies > 0;

        // Loan eklenmesi, kopya durumunun ve kitabın güncellenmesi tek bir
        // veritabanı işlemi (transaction) olarak ele alınmalı: biri başarısız
        // olursa hepsi geri alınmalı. Repository'lerin kendi SaveChangesAsync
        // çağrıları aynı DbContext'i paylaştığı için pratikte zaten birlikte
        // gidiyorlardı; burada bunu bilinçli ve açık hale getiriyoruz.
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _loanRepository.AddAsync(loan);
            await _bookRepository.UpdateAsync(book);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        var createdLoan = await _loanRepository.GetByIdAsync(loan.Id);

        if (createdLoan == null)
            return ServiceResult<LoanDto>.Fail("Loan was created but could not be retrieved.", 500);

        return ServiceResult<LoanDto>.Ok(MapToLoanDto(createdLoan));
    }

    public async Task<ServiceResult<LoanDto>> ReturnBookAsync(
        ReturnBookDto dto,
        int userId,
        bool isAdmin)
    {
        var loan = await _loanRepository.GetByIdAsync(dto.LoanId);

        if (loan == null)
            return ServiceResult<LoanDto>.Fail("Loan not found.", 404);

        if (loan.IsReturned || loan.ReturnDate != null)
            return ServiceResult<LoanDto>.Fail("This loan has already been returned.", 409);

        if (!isAdmin)
        {
            var currentMember = await _context.Members
                .FirstOrDefaultAsync(m => m.UserId == userId);

            if (currentMember == null)
                return ServiceResult<LoanDto>.Fail("No member profile found for the current user.", 404);

            if (loan.MemberId != currentMember.Id)
                return ServiceResult<LoanDto>.Fail("You are not allowed to return this loan.", 403);
        }

        loan.ReturnDate = DateTime.UtcNow;
        loan.IsReturned = true;

        var copyReturnedToShelf = dto.Condition == "Good";

        if (loan.BookCopy != null)
        {
            loan.BookCopy.Status = dto.Condition switch
            {
                "Damaged" => CopyStatus.Damaged,
                "Lost" => CopyStatus.Lost,
                _ => CopyStatus.Available
            };

            loan.BookCopy.ConditionNote = dto.ConditionNote;
        }

        if (loan.Book != null)
        {
            if (copyReturnedToShelf && loan.Book.AvailableCopies < loan.Book.TotalCopies)
            {
                loan.Book.AvailableCopies += 1;
            }

            loan.Book.IsAvailable = loan.Book.AvailableCopies > 0;
        }

        // Loan kapatma + kopya/kitap güncellemesi + ceza oluşturma (varsa)
        // tek bir transaction: cezalardan biri oluşurken hata olursa loan
        // "iade edildi" olarak yarım kalmamalı.
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _fineService.CreateFineIfNeededAsync(loan);
            await _fineService.CreateConditionFineIfNeededAsync(loan, dto.Condition);

            await _loanRepository.UpdateAsync(loan);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return ServiceResult<LoanDto>.Ok(MapToLoanDto(loan));
    }

    public async Task<IEnumerable<LoanDto>> GetMyLoansAsync(int userId)
    {
        var loans = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Include(l => l.BookCopy)
            .Where(l => l.Member.UserId == userId)
            .OrderByDescending(l => l.BorrowDate)
            .ToListAsync();

        return loans.Select(MapToLoanDto);
    }

    public async Task<IEnumerable<LoanDto>> GetLoansByMemberAsync(int memberId)
    {
        var loans = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Include(l => l.BookCopy)
            .Where(l => l.MemberId == memberId)
            .OrderByDescending(l => l.BorrowDate)
            .ToListAsync();

        return loans.Select(MapToLoanDto);
    }

    public async Task<IEnumerable<BookLoanHistoryDto>?> GetLoanHistoryByBookIdAsync(int bookId)
    {
        var book = await _bookRepository.GetByIdAsync(bookId);

        if (book == null)
            return null;

        var loans = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Include(l => l.BookCopy)
            .Where(l => l.BookId == bookId)
            .OrderByDescending(l => l.BorrowDate)
            .ToListAsync();

        return loans.Select(l => new BookLoanHistoryDto
        {
            LoanId = l.Id,
            MemberId = l.MemberId,
            MemberName = l.Member != null
                ? l.Member.FirstName + " " + l.Member.LastName
                : string.Empty,
            BookId = l.BookId,
            CopyNumber = l.BookCopy != null ? l.BookCopy.CopyNumber : 0,
            CopyStatus = l.BookCopy != null ? l.BookCopy.Status.ToString() : "Unknown",
            BorrowDate = l.BorrowDate,
            ReturnDate = l.ReturnDate,
            IsReturned = l.IsReturned
        });
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
            CopyNumber = loan.BookCopy != null ? loan.BookCopy.CopyNumber : 0,
            CopyStatus = loan.BookCopy != null ? loan.BookCopy.Status.ToString() : "Unknown",

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