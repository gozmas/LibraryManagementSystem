using LibraryManagementSystem.API.Dtos;

namespace LibraryManagementSystem.API.Services.Interfaces;

public interface ILoanService
{
    Task<IEnumerable<LoanDto>> GetAllAsync();
    Task<LoanDto?> BorrowBookAsync(BorrowBookDto dto);
    Task<LoanDto?> ReturnBookAsync(ReturnBookDto dto);
}