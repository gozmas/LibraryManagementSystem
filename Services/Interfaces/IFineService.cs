using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Models;

namespace LibraryManagementSystem.API.Services.Interfaces;

public interface IFineService
{
    Task CreateFineIfNeededAsync(Loan loan);

    Task<IEnumerable<FineDto>> GetAllAsync();

    Task<FineDto?> GetByIdAsync(int id);

    Task<bool> PayFineAsync(int id);
}