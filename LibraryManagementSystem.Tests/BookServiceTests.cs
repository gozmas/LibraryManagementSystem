using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using LibraryManagementSystem.API.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LibraryManagementSystem.Tests;

public class BookServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Book_When_Book_Exists()
    {
        // Arrange
        var mockRepository = new Mock<IBookRepository>();
        await using var context = CreateContext();

        var expectedBook = new Book
        {
            Id = 1,
            Title = "Clean Code",
            ISBN = "9780132350884",
            PublicationYear = 2008,
            IsAvailable = true,
            AuthorId = 1,
            CategoryId = 1
        };

        mockRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(expectedBook);

        var service = new BookService(mockRepository.Object, context);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Clean Code", result.Title);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task AddAsync_Should_Call_Repository_AddAsync_And_Create_Copies()
    {
        // Arrange
        var mockRepository = new Mock<IBookRepository>();
        await using var context = CreateContext();

        var service = new BookService(mockRepository.Object, context);

        var book = new Book
        {
            Id = 1,
            Title = "Test Book",
            ISBN = "TEST-001",
            PublicationYear = 2026,
            IsAvailable = true,
            TotalCopies = 3,
            AuthorId = 1,
            CategoryId = 1
        };

        // Act
        await service.AddAsync(book);

        // Assert
        mockRepository.Verify(repo => repo.AddAsync(book), Times.Once);

        // BookService.AddAsync, TotalCopies kadar BookCopy kaydı oluşturuyor
        var copyCount = await context.BookCopies.CountAsync(c => c.BookId == book.Id);
        Assert.Equal(3, copyCount);
    }

    [Fact]
    public async Task BackfillCopiesAsync_Should_Return_False_When_Book_Not_Found()
    {
        // Arrange
        var mockRepository = new Mock<IBookRepository>();
        await using var context = CreateContext();

        mockRepository
            .Setup(repo => repo.GetByIdAsync(999))
            .ReturnsAsync((Book?)null);

        var service = new BookService(mockRepository.Object, context);

        // Act
        var result = await service.BackfillCopiesAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task BackfillCopiesAsync_Should_Create_Copies_When_None_Exist()
    {
        // Arrange
        var mockRepository = new Mock<IBookRepository>();
        await using var context = CreateContext();

        var book = new Book
        {
            Id = 1,
            Title = "Legacy Book",
            ISBN = "LEGACY-001",
            PublicationYear = 2020,
            TotalCopies = 2,
            AuthorId = 1,
            CategoryId = 1
        };

        mockRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(book);

        var service = new BookService(mockRepository.Object, context);

        // Act
        var result = await service.BackfillCopiesAsync(1);

        // Assert
        Assert.True(result);

        var copyCount = await context.BookCopies.CountAsync(c => c.BookId == 1);
        Assert.Equal(2, copyCount);
    }
}