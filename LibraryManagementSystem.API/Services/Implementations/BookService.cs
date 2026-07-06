using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using LibraryManagementSystem.API.Services.Interfaces;
using LibraryManagementSystem.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.API.Services.Implementations
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly AppDbContext _context;

        public BookService(IBookRepository bookRepository, AppDbContext context)
        {
            _bookRepository = bookRepository;
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _bookRepository.GetAllAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _bookRepository.GetByIdAsync(id);
        }

        public async Task AddAsync(Book book)
        {
            await _bookRepository.AddAsync(book);
            await CreateCopiesAsync(book.Id, book.TotalCopies);
        }

        public async Task UpdateAsync(Book book)
        {
            await _bookRepository.UpdateAsync(book);
            await SyncCopiesAsync(book.Id, book.TotalCopies);
        }

        public async Task DeleteAsync(Book book)
        {
            await _bookRepository.DeleteAsync(book);
        }

        public async Task<PagedResultDto<BookListDto>> SearchBooksAsync(BookQueryDto query)
        {
            return await _bookRepository.SearchBooksAsync(query);
        }

        private async Task CreateCopiesAsync(int bookId, int totalCopies)
        {
            for (int i = 1; i <= totalCopies; i++)
            {
                _context.BookCopies.Add(new BookCopy
                {
                    BookId = bookId,
                    CopyNumber = i,
                    Status = CopyStatus.Available
                });
            }

            await _context.SaveChangesAsync();
        }

        private async Task SyncCopiesAsync(int bookId, int newTotalCopies)
        {
            var existingCount = await _context.BookCopies
                .CountAsync(c => c.BookId == bookId);

            if (newTotalCopies <= existingCount)
                return;

            for (int i = existingCount + 1; i <= newTotalCopies; i++)
            {
                _context.BookCopies.Add(new BookCopy
                {
                    BookId = bookId,
                    CopyNumber = i,
                    Status = CopyStatus.Available
                });
            }

            await _context.SaveChangesAsync();
        }
        public async Task<bool> BackfillCopiesAsync(int bookId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);

            if (book == null)
                return false;

            var existingCopies = await _context.BookCopies
                .Where(c => c.BookId == bookId)
                .ToListAsync();

            if (existingCopies.Count > 0)
                return true;

            var newCopies = new List<BookCopy>();

            for (int i = 1; i <= book.TotalCopies; i++)
            {
                var copy = new BookCopy
                {
                    BookId = bookId,
                    CopyNumber = i,
                    Status = CopyStatus.Available
                };

                newCopies.Add(copy);
                _context.BookCopies.Add(copy);
            }

            await _context.SaveChangesAsync();

            var unlinkedActiveLoans = await _context.Loans
                .Where(l => l.BookId == bookId && l.BookCopyId == null && !l.IsReturned)
                .OrderBy(l => l.BorrowDate)
                .ToListAsync();

            for (int i = 0; i < unlinkedActiveLoans.Count && i < newCopies.Count; i++)
            {
                unlinkedActiveLoans[i].BookCopyId = newCopies[i].Id;
                newCopies[i].Status = CopyStatus.Borrowed;
            }

            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<int> BackfillAllCopiesAsync()
        {
            var allBooks = await _context.Books.ToListAsync();
            var count = 0;

            foreach (var book in allBooks)
            {
                var success = await BackfillCopiesAsync(book.Id);

                if (success)
                    count++;
            }

            return count;
        }
    }
}