using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using LibraryManagementSystem.API.Services.Implementations;
using LibraryManagementSystem.API.Services.Interfaces;
using Moq;
using Xunit;

namespace LibraryManagementSystem.Tests;

public class LoanServiceTests
{
    [Fact]
    public async Task BorrowBookAsync_Should_Return_Null_When_Book_Not_Found()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var bookRepositoryMock = new Mock<IBookRepository>();
        var fineServiceMock = new Mock<IFineService>();

        var dto = new BorrowBookDto
        {
            BookId = 999,
            MemberId = 1
        };

        bookRepositoryMock
            .Setup(repo => repo.GetByIdAsync(dto.BookId))
            .ReturnsAsync((Book?)null);

        var service = new LoanService(
            loanRepositoryMock.Object,
            bookRepositoryMock.Object,
            fineServiceMock.Object);

        // Act
        var result = await service.BorrowBookAsync(dto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task BorrowBookAsync_Should_Return_LoanDto_When_Book_Is_Available()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var bookRepositoryMock = new Mock<IBookRepository>();
        var fineServiceMock = new Mock<IFineService>();

        var dto = new BorrowBookDto
        {
            BookId = 1,
            MemberId = 1
        };

        var book = new Book
        {
            Id = 1,
            Title = "Clean Code",
            ISBN = "9780132350884",
            PublicationYear = 2008,
            IsAvailable = true,
            AuthorId = 1,
            CategoryId = 1
        };

        var createdLoan = new Loan
        {
            Id = 1,
            BookId = 1,
            Book = book,
            MemberId = 1,
            Member = new Member
            {
                Id = 1,
                FirstName = "Gozde",
                LastName = "Yilikyilmaz",
                Email = "gozde@test.com"
            },
            BorrowDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(14),
            IsReturned = false
        };

        bookRepositoryMock
            .Setup(repo => repo.GetByIdAsync(dto.BookId))
            .ReturnsAsync(book);

        loanRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Loan>()))
            .Returns(Task.CompletedTask)
            .Callback<Loan>(loan =>
            {
                loan.Id = 1;
            });

        bookRepositoryMock
            .Setup(repo => repo.UpdateAsync(book))
            .Returns(Task.CompletedTask);

        loanRepositoryMock
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(createdLoan);

        var service = new LoanService(
            loanRepositoryMock.Object,
            bookRepositoryMock.Object,
            fineServiceMock.Object);

        // Act
        var result = await service.BorrowBookAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.BookId);
        Assert.Equal("Clean Code", result.BookTitle);
        Assert.Equal("Gozde Yilikyilmaz", result.MemberName);
        Assert.False(book.IsAvailable);

        loanRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Loan>()), Times.Once);
        bookRepositoryMock.Verify(repo => repo.UpdateAsync(book), Times.Once);
    }
    [Fact]
public async Task ReturnBookAsync_Should_Return_Null_When_Loan_Not_Found()
{
    // Arrange
    var loanRepositoryMock = new Mock<ILoanRepository>();
    var bookRepositoryMock = new Mock<IBookRepository>();
    var fineServiceMock = new Mock<IFineService>();

    var dto = new ReturnBookDto
    {
        LoanId = 999
    };

    loanRepositoryMock
        .Setup(repo => repo.GetByIdAsync(dto.LoanId))
        .ReturnsAsync((Loan?)null);

    var service = new LoanService(
        loanRepositoryMock.Object,
        bookRepositoryMock.Object,
        fineServiceMock.Object);

    // Act
    var result = await service.ReturnBookAsync(dto);

    // Assert
    Assert.Null(result);
}

[Fact]
public async Task ReturnBookAsync_Should_Return_Null_When_Loan_Already_Returned()
{
    // Arrange
    var loanRepositoryMock = new Mock<ILoanRepository>();
    var bookRepositoryMock = new Mock<IBookRepository>();
    var fineServiceMock = new Mock<IFineService>();

    var dto = new ReturnBookDto
    {
        LoanId = 1
    };

    var loan = new Loan
    {
        Id = 1,
        BookId = 1,
        MemberId = 1,
        IsReturned = true
    };

    loanRepositoryMock
        .Setup(repo => repo.GetByIdAsync(dto.LoanId))
        .ReturnsAsync(loan);

    var service = new LoanService(
        loanRepositoryMock.Object,
        bookRepositoryMock.Object,
        fineServiceMock.Object);

    // Act
    var result = await service.ReturnBookAsync(dto);

    // Assert
    Assert.Null(result);
}

[Fact]
public async Task ReturnBookAsync_Should_Return_LoanDto_When_Loan_Is_Returned_Successfully()
{
    // Arrange
    var loanRepositoryMock = new Mock<ILoanRepository>();
    var bookRepositoryMock = new Mock<IBookRepository>();
    var fineServiceMock = new Mock<IFineService>();

    var dto = new ReturnBookDto
    {
        LoanId = 1
    };

    var book = new Book
    {
        Id = 1,
        Title = "Clean Code",
        ISBN = "9780132350884",
        PublicationYear = 2008,
        IsAvailable = false,
        AuthorId = 1,
        CategoryId = 1
    };

    var loan = new Loan
    {
        Id = 1,
        BookId = 1,
        Book = book,
        MemberId = 1,
        Member = new Member
        {
            Id = 1,
            FirstName = "Gozde",
            LastName = "Yilikyilmaz",
            Email = "gozde@test.com"
        },
        BorrowDate = DateTime.UtcNow.AddDays(-10),
        DueDate = DateTime.UtcNow.AddDays(4),
        IsReturned = false
    };

    loanRepositoryMock
        .Setup(repo => repo.GetByIdAsync(dto.LoanId))
        .ReturnsAsync(loan);

    fineServiceMock
        .Setup(service => service.CreateFineIfNeededAsync(loan))
        .Returns(Task.CompletedTask);

    loanRepositoryMock
        .Setup(repo => repo.UpdateAsync(loan))
        .Returns(Task.CompletedTask);

    var service = new LoanService(
        loanRepositoryMock.Object,
        bookRepositoryMock.Object,
        fineServiceMock.Object);

    // Act
    var result = await service.ReturnBookAsync(dto);

    // Assert
    Assert.NotNull(result);
    Assert.True(result.IsReturned);
    Assert.NotNull(result.ReturnDate);
    Assert.True(book.IsAvailable);
    Assert.Equal("Clean Code", result.BookTitle);
    Assert.Equal("Gozde Yilikyilmaz", result.MemberName);

    fineServiceMock.Verify(service => service.CreateFineIfNeededAsync(loan), Times.Once);
    loanRepositoryMock.Verify(repo => repo.UpdateAsync(loan), Times.Once);
}
}