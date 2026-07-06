using LibraryManagementSystem.API.Dtos;

namespace LibraryManagementSystem.API.Services.Interfaces;

public interface ILoanService
{
    Task<IEnumerable<LoanDto>> GetAllAsync();

    Task<LoanDto?> BorrowBookAsync(
        BorrowBookDto dto,
        int userId,
        bool isAdmin);

    Task<LoanDto?> ReturnBookAsync(
        ReturnBookDto dto,
        int userId,
        bool isAdmin);

    Task<IEnumerable<LoanDto>> GetMyLoansAsync(int userId);

    Task<IEnumerable<LoanDto>> GetLoansByMemberAsync(int memberId);

    Task<IEnumerable<BookLoanHistoryDto>?> GetLoanHistoryByBookIdAsync(int bookId);
}