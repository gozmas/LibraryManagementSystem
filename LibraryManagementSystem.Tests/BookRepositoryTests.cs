using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LibraryManagementSystem.Tests;

public class BookRepositoryTests
{
    [Fact]
    public async Task AddAsync_Should_Add_Book_To_Database()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        context.Authors.Add(new Author
        {
            Id = 1,
            FirstName = "Robert",
            LastName = "Martin"
        });

        context.Categories.Add(new Category
        {
            Id = 1,
            Name = "Software Engineering"
        });

        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        var book = new Book
        {
            Title = "Unit Test Book",
            ISBN = "UNIT-001",
            PublicationYear = 2026,
            IsAvailable = true,
            AuthorId = 1,
            CategoryId = 1
        };

        // Act
        await repository.AddAsync(book);

        // Assert
        Assert.Equal(1, await context.Books.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Book_When_Book_Exists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var context = new AppDbContext(options);

        context.Authors.Add(new Author
        {
            Id = 1,
            FirstName = "Robert",
            LastName = "Martin"
        });

        context.Categories.Add(new Category
        {
            Id = 1,
            Name = "Software Engineering"
        });

        context.Books.Add(new Book
        {
            Id = 1,
            Title = "Clean Code",
            ISBN = "9780132350884",
            PublicationYear = 2008,
            IsAvailable = true,
            AuthorId = 1,
            CategoryId = 1
        });

        await context.SaveChangesAsync();

        var repository = new BookRepository(context);

        // Act
        var result = await repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Clean Code", result.Title);
        Assert.Equal("Robert", result.Author.FirstName);
        Assert.Equal("Software Engineering", result.Category.Name);
    }
}