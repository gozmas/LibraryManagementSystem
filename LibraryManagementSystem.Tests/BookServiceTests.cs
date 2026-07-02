using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using LibraryManagementSystem.API.Services.Implementations;
using Moq;
using Xunit;

namespace LibraryManagementSystem.Tests;

public class BookServiceTests
{
    [Fact]
    public async Task GetByIdAsync_Should_Return_Book_When_Book_Exists()
    {
        // Arrange
        var mockRepository = new Mock<IBookRepository>();

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

        var service = new BookService(mockRepository.Object);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Clean Code", result.Title);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task AddAsync_Should_Call_Repository_AddAsync()
    {
        // Arrange
        var mockRepository = new Mock<IBookRepository>();

        var service = new BookService(mockRepository.Object);

        var book = new Book
        {
            Title = "Test Book",
            ISBN = "TEST-001",
            PublicationYear = 2026,
            IsAvailable = true,
            AuthorId = 1,
            CategoryId = 1
        };

        // Act
        await service.AddAsync(book);

        // Assert
        mockRepository.Verify(repo => repo.AddAsync(book), Times.Once);
    }
}