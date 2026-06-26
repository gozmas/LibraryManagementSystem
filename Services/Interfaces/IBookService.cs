using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Dtos;

namespace LibraryManagementSystem.API.Services.Interfaces
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task<Book?> GetByIdAsync(int id);
        Task AddAsync(Book book);
        Task UpdateAsync(Book book);
        Task DeleteAsync(Book book);
        Task<PagedResultDto<BookListDto>> SearchBooksAsync(BookQueryDto query);
    }
}