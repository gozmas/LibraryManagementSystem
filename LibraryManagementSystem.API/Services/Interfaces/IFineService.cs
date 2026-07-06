using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Models;

namespace LibraryManagementSystem.API.Services.Interfaces;

public interface IFineService
{
    Task CreateFineIfNeededAsync(Loan loan);

    Task CreateConditionFineIfNeededAsync(Loan loan, string condition);

    Task<IEnumerable<FineDto>> GetAllAsync();

    Task<FineDto?> GetByIdAsync(int id);

    Task<IEnumerable<FineDto>> GetMyFinesAsync(int userId);

    Task<bool> PayFineAsync(
        int id,
        int userId,
        bool isAdmin);
}