using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Dtos;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using LibraryManagementSystem.API.Services.Implementations;
using LibraryManagementSystem.API.Hubs;
using LibraryManagementSystem.API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LibraryManagementSystem.Tests;

// NOT: LoanService, _context.Members / _context.BookCopies gibi DbSet'lere
// doğrudan LINQ sorgusu attığı için Moq ile mocklanamıyor (IQueryable async
// sorgular gerektiriyor). Onun yerine EF Core InMemory provider ile gerçek
// bir AppDbContext instance'ı kullanıyoruz - her test kendi izole
// veritabanı adını alıyor (Guid ile) ki testler birbirini etkilemesin.
public class LoanServiceTests
{
   private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    // LoanService, borrow/return sonrası IHubContext<LoanHub> üzerinden
    // SignalR event'i yayınlıyor. Testlerde gerçek bir hub olmadığı için
    // Clients.All ve Clients.User(...) çağrılarının no-op bir IClientProxy
    // döndüreceği sahte (stub) bir hub context oluşturuyoruz.
    private static IHubContext<LoanHub> CreateHubContextMock()
    {
        var clientProxyMock = new Mock<IClientProxy>();
        clientProxyMock
            .Setup(proxy => proxy.SendCoreAsync(
                It.IsAny<string>(),
                It.IsAny<object[]>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var clientsMock = new Mock<IHubClients>();
        clientsMock.Setup(c => c.All).Returns(clientProxyMock.Object);
        clientsMock.Setup(c => c.User(It.IsAny<string>())).Returns(clientProxyMock.Object);

        var hubContextMock = new Mock<IHubContext<LoanHub>>();
        hubContextMock.Setup(h => h.Clients).Returns(clientsMock.Object);

        return hubContextMock.Object;
    }

    [Fact]
    public async Task BorrowBookAsync_Should_Fail_When_Book_Not_Found()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var bookRepositoryMock = new Mock<IBookRepository>();
        var fineServiceMock = new Mock<IFineService>();
        var wishlistRepositoryMock = new Mock<IWishlistRepository>();
        wishlistRepositoryMock
            .Setup(repo => repo.GetMembersWishlistingBookAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Member>());
        await using var context = CreateContext();

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
            fineServiceMock.Object,
            wishlistRepositoryMock.Object,
            context,
            CreateHubContextMock());

        // Act
        var result = await service.BorrowBookAsync(dto, userId: 1, isAdmin: true);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Book not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task BorrowBookAsync_Should_Fail_When_No_Copy_Available()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var bookRepositoryMock = new Mock<IBookRepository>();
        var fineServiceMock = new Mock<IFineService>();
        var wishlistRepositoryMock = new Mock<IWishlistRepository>();
        wishlistRepositoryMock
            .Setup(repo => repo.GetMembersWishlistingBookAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Member>());
        await using var context = CreateContext();

        var book = new Book
        {
            Id = 1,
            Title = "Clean Code",
            ISBN = "9780132350884",
            PublicationYear = 2008,
            IsAvailable = true,
            AvailableCopies = 1,
            TotalCopies = 1,
            AuthorId = 1,
            CategoryId = 1
        };

        // Kasıtlı olarak Available durumda bir BookCopy eklemiyoruz,
        // böylece "kopya bulunamadı" dalını test ediyoruz.
        context.Members.Add(new Member { Id = 1, FirstName = "Gozde", LastName = "Yilikyilmaz", Email = "gozde@test.com" });
        await context.SaveChangesAsync();

        var dto = new BorrowBookDto { BookId = 1, MemberId = 1 };

        bookRepositoryMock
            .Setup(repo => repo.GetByIdAsync(dto.BookId))
            .ReturnsAsync(book);

        var service = new LoanService(
            loanRepositoryMock.Object,
            bookRepositoryMock.Object,
            fineServiceMock.Object,
            wishlistRepositoryMock.Object,
            context,
            CreateHubContextMock());

        // Act
        var result = await service.BorrowBookAsync(dto, userId: 1, isAdmin: true);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task BorrowBookAsync_Should_Return_LoanDto_When_Book_Is_Available()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var bookRepositoryMock = new Mock<IBookRepository>();
        var fineServiceMock = new Mock<IFineService>();
        var wishlistRepositoryMock = new Mock<IWishlistRepository>();
        wishlistRepositoryMock
            .Setup(repo => repo.GetMembersWishlistingBookAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Member>());
        await using var context = CreateContext();

        var book = new Book
        {
            Id = 1,
            Title = "Clean Code",
            ISBN = "9780132350884",
            PublicationYear = 2008,
            IsAvailable = true,
            AvailableCopies = 1,
            TotalCopies = 1,
            AuthorId = 1,
            CategoryId = 1
        };

        var member = new Member { Id = 1, FirstName = "Gozde", LastName = "Yilikyilmaz", Email = "gozde@test.com" };
        var copy = new BookCopy { Id = 1, BookId = 1, CopyNumber = 1, Status = CopyStatus.Available };

        context.Members.Add(member);
        context.BookCopies.Add(copy);
        await context.SaveChangesAsync();

        var dto = new BorrowBookDto { BookId = 1, MemberId = 1 };

        var createdLoan = new Loan
        {
            Id = 1,
            BookId = 1,
            Book = book,
            MemberId = 1,
            Member = member,
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
            .Callback<Loan>(loan => loan.Id = 1);

        bookRepositoryMock
            .Setup(repo => repo.UpdateAsync(book))
            .Returns(Task.CompletedTask);

        loanRepositoryMock
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(createdLoan);

        var service = new LoanService(
            loanRepositoryMock.Object,
            bookRepositoryMock.Object,
            fineServiceMock.Object,
            wishlistRepositoryMock.Object,
            context,
            CreateHubContextMock());

        // Act
        // Not: InMemory provider gerçek transaction desteklemez, bu yüzden
        // BeginTransactionAsync burada no-op bir transaction döner - test
        // akışını bozmaz.
        var result = await service.BorrowBookAsync(dto, userId: 1, isAdmin: true);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(1, result.Data!.BookId);
        Assert.Equal("Clean Code", result.Data.BookTitle);
        Assert.Equal("Gozde Yilikyilmaz", result.Data.MemberName);
        Assert.False(book.IsAvailable);

        loanRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Loan>()), Times.Once);
        bookRepositoryMock.Verify(repo => repo.UpdateAsync(book), Times.Once);
    }

    [Fact]
    public async Task ReturnBookAsync_Should_Fail_When_Loan_Not_Found()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var bookRepositoryMock = new Mock<IBookRepository>();
        var fineServiceMock = new Mock<IFineService>();
        var wishlistRepositoryMock = new Mock<IWishlistRepository>();
        wishlistRepositoryMock
            .Setup(repo => repo.GetMembersWishlistingBookAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Member>());
        await using var context = CreateContext();

        var dto = new ReturnBookDto { LoanId = 999 };

        loanRepositoryMock
            .Setup(repo => repo.GetByIdAsync(dto.LoanId))
            .ReturnsAsync((Loan?)null);

        var service = new LoanService(
            loanRepositoryMock.Object,
            bookRepositoryMock.Object,
            fineServiceMock.Object,
            wishlistRepositoryMock.Object,
            context,
            CreateHubContextMock());

        // Act
        var result = await service.ReturnBookAsync(dto, userId: 1, isAdmin: true);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task ReturnBookAsync_Should_Fail_When_Loan_Already_Returned()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var bookRepositoryMock = new Mock<IBookRepository>();
        var fineServiceMock = new Mock<IFineService>();
        var wishlistRepositoryMock = new Mock<IWishlistRepository>();
        wishlistRepositoryMock
            .Setup(repo => repo.GetMembersWishlistingBookAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Member>());
        await using var context = CreateContext();

        var dto = new ReturnBookDto { LoanId = 1 };

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
            fineServiceMock.Object,
            wishlistRepositoryMock.Object,
            context,
            CreateHubContextMock());

        // Act
        var result = await service.ReturnBookAsync(dto, userId: 1, isAdmin: true);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task ReturnBookAsync_Should_Return_LoanDto_When_Loan_Is_Returned_Successfully()
    {
        // Arrange
        var loanRepositoryMock = new Mock<ILoanRepository>();
        var bookRepositoryMock = new Mock<IBookRepository>();
        var fineServiceMock = new Mock<IFineService>();
        var wishlistRepositoryMock = new Mock<IWishlistRepository>();
        wishlistRepositoryMock
            .Setup(repo => repo.GetMembersWishlistingBookAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Member>());
        await using var context = CreateContext();

        var dto = new ReturnBookDto { LoanId = 1, Condition = "Good" };

        var book = new Book
        {
            Id = 1,
            Title = "Clean Code",
            ISBN = "9780132350884",
            PublicationYear = 2008,
            IsAvailable = false,
            AvailableCopies = 0,
            TotalCopies = 1,
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

        fineServiceMock
            .Setup(service => service.CreateConditionFineIfNeededAsync(loan, dto.Condition))
            .Returns(Task.CompletedTask);

        loanRepositoryMock
            .Setup(repo => repo.UpdateAsync(loan))
            .Returns(Task.CompletedTask);

        var service = new LoanService(
            loanRepositoryMock.Object,
            bookRepositoryMock.Object,
            fineServiceMock.Object,
            wishlistRepositoryMock.Object,
            context,
            CreateHubContextMock());

        // Act
        var result = await service.ReturnBookAsync(dto, userId: 1, isAdmin: true);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data!.IsReturned);
        Assert.NotNull(result.Data.ReturnDate);
        Assert.True(book.IsAvailable);
        Assert.Equal("Clean Code", result.Data.BookTitle);
        Assert.Equal("Gozde Yilikyilmaz", result.Data.MemberName);

        fineServiceMock.Verify(service => service.CreateFineIfNeededAsync(loan), Times.Once);
        loanRepositoryMock.Verify(repo => repo.UpdateAsync(loan), Times.Once);
    }
}