using LibraryManagementSystem.API.Data;
using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using LibraryManagementSystem.API.Services.Implementations;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LibraryManagementSystem.Tests;

// NOT: FineService, ceza sorgularını (PayFineAsync, CreateFineIfNeededAsync,
// CreateConditionFineIfNeededAsync) doğrudan _context üzerinden yapıyor,
// _fineRepository sadece AddAsync/UpdateAsync için kullanılıyor. Bu yüzden
// context'i mocklamak yerine EF Core InMemory ile gerçek context kullanıyoruz.
public class FineServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task PayFineAsync_Should_Return_False_When_Fine_Not_Found()
    {
        var fineRepositoryMock = new Mock<IFineRepository>();
        await using var context = CreateContext();

        var service = new FineService(fineRepositoryMock.Object, context);

        var result = await service.PayFineAsync(999, userId: 1, isAdmin: true);

        Assert.False(result.Success);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task PayFineAsync_Should_Return_False_When_Fine_Already_Paid()
    {
        var fineRepositoryMock = new Mock<IFineRepository>();
        await using var context = CreateContext();

        var loan = new Loan { Id = 1, BookId = 1, MemberId = 1 };
        var fine = new Fine { Id = 1, LoanId = 1, Loan = loan, Amount = 10, IsPaid = true };

        context.Loans.Add(loan);
        context.Fines.Add(fine);
        await context.SaveChangesAsync();

        var service = new FineService(fineRepositoryMock.Object, context);

        var result = await service.PayFineAsync(1, userId: 1, isAdmin: true);

        Assert.False(result.Success);
        Assert.Equal(409, result.StatusCode);
    }

    [Fact]
    public async Task PayFineAsync_Should_Return_True_And_Update_Fine_When_Fine_Is_Unpaid()
    {
        var fineRepositoryMock = new Mock<IFineRepository>();
        await using var context = CreateContext();

        var loan = new Loan { Id = 1, BookId = 1, MemberId = 1 };
        var fine = new Fine { Id = 1, LoanId = 1, Loan = loan, Amount = 10, IsPaid = false };

        context.Loans.Add(loan);
        context.Fines.Add(fine);
        await context.SaveChangesAsync();

        fineRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Fine>()))
            .Returns(Task.CompletedTask);

        var service = new FineService(fineRepositoryMock.Object, context);

        // isAdmin: true -> üye eşleşme kontrolü atlanır
        var result = await service.PayFineAsync(1, userId: 1, isAdmin: true);

        Assert.True(result.Success);

        var updatedFine = await context.Fines.FindAsync(1);
        Assert.NotNull(updatedFine);
        Assert.True(updatedFine!.IsPaid);

        fineRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Fine>()), Times.Once);
    }

    [Fact]
    public async Task CreateFineIfNeededAsync_Should_Not_Create_Fine_When_ReturnDate_Is_Null()
    {
        var fineRepositoryMock = new Mock<IFineRepository>();
        await using var context = CreateContext();

        var loan = new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            DueDate = DateTime.UtcNow.AddDays(-5),
            ReturnDate = null
        };

        var service = new FineService(fineRepositoryMock.Object, context);

        await service.CreateFineIfNeededAsync(loan);

        fineRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Fine>()), Times.Never);
    }

    [Fact]
    public async Task CreateFineIfNeededAsync_Should_Not_Create_Fine_When_Returned_On_Time()
    {
        var fineRepositoryMock = new Mock<IFineRepository>();
        await using var context = CreateContext();

        var loan = new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            DueDate = DateTime.UtcNow,
            ReturnDate = DateTime.UtcNow
        };

        var service = new FineService(fineRepositoryMock.Object, context);

        await service.CreateFineIfNeededAsync(loan);

        fineRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Fine>()), Times.Never);
    }

    [Fact]
    public async Task CreateFineIfNeededAsync_Should_Create_Fine_When_Returned_Late()
    {
        var fineRepositoryMock = new Mock<IFineRepository>();
        await using var context = CreateContext();

        var dueDate = new DateTime(2026, 6, 22);      // Pazartesi
        var returnDate = new DateTime(2026, 6, 25);   // Perşembe

        var loan = new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            DueDate = dueDate,
            ReturnDate = returnDate
        };

        Fine? createdFine = null;

        fineRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Fine>()))
            .Callback<Fine>(fine => createdFine = fine)
            .Returns(Task.CompletedTask);

        var service = new FineService(fineRepositoryMock.Object, context);

        await service.CreateFineIfNeededAsync(loan);

        fineRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Fine>()), Times.Once);

        Assert.NotNull(createdFine);
        Assert.Equal(1, createdFine!.LoanId);
        Assert.Equal(15, createdFine.Amount);
        Assert.False(createdFine.IsPaid);
        Assert.Equal(FineReason.Late, createdFine.Reason);
    }

    [Fact]
    public async Task CreateFineIfNeededAsync_Should_Not_Duplicate_Fine_When_Late_Fine_Already_Exists()
    {
        var fineRepositoryMock = new Mock<IFineRepository>();
        await using var context = CreateContext();

        var loan = new Loan
        {
            Id = 1,
            BookId = 1,
            MemberId = 1,
            DueDate = new DateTime(2026, 6, 22),
            ReturnDate = new DateTime(2026, 6, 25)
        };

        context.Loans.Add(loan);
        context.Fines.Add(new Fine { Id = 1, LoanId = 1, Loan = loan, Amount = 15, Reason = FineReason.Late });
        await context.SaveChangesAsync();

        var service = new FineService(fineRepositoryMock.Object, context);

        await service.CreateFineIfNeededAsync(loan);

        // Zaten bir "Late" ceza kaydı olduğu için ikinci bir tane oluşturulmamalı
        fineRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Fine>()), Times.Never);
    }
}