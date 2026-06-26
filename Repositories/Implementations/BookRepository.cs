using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Repositories.Implementations
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _context;

        public BookRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _context.Books.ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task AddAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Book book)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        public async Task<PagedResultDto<BookListDto>> SearchBooksAsync(BookQueryDto query)
        {
            var booksQuery = _context.Books
                .Include(b => b.Author)
                .Include(b => b.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                booksQuery = booksQuery.Where(b =>
                    b.Title.Contains(query.SearchTerm) ||
                    b.ISBN.Contains(query.SearchTerm));
            }

            if (query.AuthorId.HasValue)
            {
                booksQuery = booksQuery.Where(b => b.AuthorId == query.AuthorId.Value);
            }

            if (query.CategoryId.HasValue)
            {
                booksQuery = booksQuery.Where(b => b.CategoryId == query.CategoryId.Value);
            }

            if (query.IsAvailable.HasValue)
            {
                booksQuery = booksQuery.Where(b => b.IsAvailable == query.IsAvailable.Value);
            }

            var sortBy = query.SortBy?.Trim().ToLower();
            var sortDirection = query.SortDirection?.Trim().ToLower();

            booksQuery = sortBy switch
            {
                "title" => sortDirection == "desc"
                    ? booksQuery.OrderByDescending(b => b.Title)
                    : booksQuery.OrderBy(b => b.Title),

                "publicationyear" => sortDirection == "desc"
                    ? booksQuery.OrderByDescending(b => b.PublicationYear)
                    : booksQuery.OrderBy(b => b.PublicationYear),

                "isbn" => sortDirection == "desc"
                    ? booksQuery.OrderByDescending(b => b.ISBN)
                    : booksQuery.OrderBy(b => b.ISBN),

                "author" => sortDirection == "desc"
                    ? booksQuery.OrderByDescending(b => b.Author.FirstName)
                    : booksQuery.OrderBy(b => b.Author.FirstName),

                "category" => sortDirection == "desc"
                    ? booksQuery.OrderByDescending(b => b.Category.Name)
                    : booksQuery.OrderBy(b => b.Category.Name),

                _ => booksQuery.OrderBy(b => b.Id)
            };

            var totalCount = await booksQuery.CountAsync();

            var books = await booksQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(b => new BookListDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    ISBN = b.ISBN,
                    PublicationYear = b.PublicationYear,
                    IsAvailable = b.IsAvailable,
                    AuthorId = b.AuthorId,
                    AuthorName = b.Author.FirstName + " " + b.Author.LastName,
                    CategoryId = b.CategoryId,
                    CategoryName = b.Category.Name
                })
                .ToListAsync();

            return new PagedResultDto<BookListDto>
            {
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize),
                Items = books
            };
        }
    }
}