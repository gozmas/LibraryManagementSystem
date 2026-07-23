using LibraryManagementSystem.API.Common;
using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Hubs;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using LibraryManagementSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Services.Implementations;

public class LoanService : ILoanService
{
   private const int MaxActiveLoansPerMember = 3;
    private const int StudentLoanDurationDays = 21;
    private const int AcademicLoanDurationDays = 30;
    private const int DefaultLoanDurationDays = 10;
    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IFineService _fineService;
    private readonly IWishlistRepository _wishlistRepository;
    private readonly AppDbContext _context;
    private readonly IHubContext<LoanHub> _hubContext;

    public LoanService(
        ILoanRepository loanRepository,
        IBookRepository bookRepository,
        IFineService fineService,
        IWishlistRepository wishlistRepository,
        AppDbContext context,
        IHubContext<LoanHub> hubContext)
    {
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
        _fineService = fineService;
        _wishlistRepository = wishlistRepository;
        _context = context;
        _hubContext = hubContext;
    }

    // Kitap durumu (Available/Total copies) her değiştiğinde bağlı olan
    // Live Activity ekranlarına anlık olarak yayınlanıyor. Metod private
    // tutuluyor çünkü sadece bu servis içindeki borrow/return akışlarından
    // tetiklenmesi gerekiyor.
    private Task BroadcastBookStatusChangedAsync(Book book, string action)
    {
        return _hubContext.Clients.All.SendAsync("BookStatusChanged", new
        {
            bookId = book.Id,
            bookTitle = book.Title,
            totalCopies = book.TotalCopies,
            availableCopies = book.AvailableCopies,
            action,
            timestamp = DateTime.UtcNow
        });
    }

    // Bir kitap iade edilip tekrar müsait hale geldiğinde, o kitabı
    // wishlist'inde bulunduran member'lara SignalR üzerinden hedefli
    // (herkese değil, sadece ilgili kullanıcıya) bildirim gönderiliyor.
    // "Clients.User(...)" JWT'deki NameIdentifier claim'ini (User.Id)
    // kullanıyor; bu yüzden Member değil, Member.UserId hedefleniyor.
    private async Task NotifyWishlistersAsync(Book book)
    {
        var wishlisters = await _wishlistRepository.GetMembersWishlistingBookAsync(book.Id);

        var notifications = wishlisters
            .Where(member => member.UserId.HasValue)
            .Select(member => _hubContext.Clients
                .User(member.UserId!.Value.ToString())
                .SendAsync("WishlistBookAvailable", new
                {
                    bookId = book.Id,
                    bookTitle = book.Title,
                    availableCopies = book.AvailableCopies,
                    totalCopies = book.TotalCopies,
                    timestamp = DateTime.UtcNow
                }));

        await Task.WhenAll(notifications);
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

        var activeLoanCount = await _context.Loans
            .CountAsync(l => l.MemberId == memberId && !l.IsReturned);

        if (activeLoanCount >= MaxActiveLoansPerMember)
        {
            return ServiceResult<LoanDto>.Fail(
                $"This member already has {MaxActiveLoansPerMember} active loans. Return a book before borrowing another.",
                409);
        }

        // Öğrenci üyeler için daha kısa, diğer üye tipleri (ör. akademisyen/
        // personel) için daha uzun ödünç süresi uygulanıyor.
        var memberRole = await _context.Members
            .Where(m => m.Id == memberId)
            .Select(m => m.User != null ? m.User.Role : "Member")
            .FirstOrDefaultAsync();

        var loanDurationDays = memberRole switch
        {
            "Student" => StudentLoanDurationDays,
            "Academic" => AcademicLoanDurationDays,
            _ => DefaultLoanDurationDays
        };

        var loan = new Loan
        {
            BookId = dto.BookId,
            BookCopyId = availableCopy.Id,
            MemberId = memberId,
            BorrowDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(loanDurationDays),
            IsReturned = false
        };

        availableCopy.Status = CopyStatus.Borrowed;

        book.AvailableCopies -= 1;
        book.IsAvailable = book.AvailableCopies > 0;

       
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

        await BroadcastBookStatusChangedAsync(book, "Borrowed");

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

        if (loan.Book != null)
        {
            await BroadcastBookStatusChangedAsync(loan.Book, "Returned");

            // Sadece kitap gerçekten rafa geri döndüyse (Damaged/Lost değilse)
            // ve şu an müsait kopyası varsa wishlist'tekilere haber ver.
            if (copyReturnedToShelf && loan.Book.AvailableCopies > 0)
            {
                await NotifyWishlistersAsync(loan.Book);
            }
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