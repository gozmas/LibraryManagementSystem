using LibraryManagementSystem.API.Models;
using LibraryManagementSystem.API.Repositories.Interfaces;
using LibraryManagementSystem.API.Services.Implementations;
using Moq;
using Xunit;

namespace LibraryManagementSystem.Tests;

public class FineServiceTests
{
    [Fact]
    public async Task PayFineAsync_Should_Return_False_When_Fine_Not_Found()
    {
        var fineRepositoryMock = new Mock<IFineRepository>();

        fineRepositoryMock
            .Setup(repo => repo.GetByIdAsync(999))
            .ReturnsAsync((Fine?)null);

        var service = new FineService(fineRepositoryMock.Object);

        var result = await service.PayFineAsync(999);

        Assert.False(result);
    }

    [Fact]
    public async Task PayFineAsync_Should_Return_False_When_Fine_Already_Paid()
    {
        var fineRepositoryMock = new Mock<IFineRepository>();

        var fine = new Fine
        {
            Id = 1,
            LoanId = 1,
            Amount = 10,
            IsPaid = true
        };

        fineRepositoryMock
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(fine);

        var service = new FineService(fineRepositoryMock.Object);

        var result = await service.PayFineAsync(1);

        Assert.False(result);
    }

    [Fact]
    public async Task PayFineAsync_Should_Return_True_And_Update_Fine_When_Fine_Is_Unpaid()
    {
        var fineRepositoryMock = new Mock<IFineRepository>();

        var fine = new Fine
        {
            Id = 1,
            LoanId = 1,
            Amount = 10,
            IsPaid = false
        };

        fineRepositoryMock
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(fine);

        fineRepositoryMock
            .Setup(repo => repo.UpdateAsync(fine))
            .Returns(Task.CompletedTask);

        var service = new FineService(fineRepositoryMock.Object);

        var result = await service.PayFineAsync(1);

        Assert.True(result);
        Assert.True(fine.IsPaid);

        fineRepositoryMock.Verify(repo => repo.UpdateAsync(fine), Times.Once);
    }
    [Fact]
public async Task CreateFineIfNeededAsync_Should_Not_Create_Fine_When_ReturnDate_Is_Null()
{
    var fineRepositoryMock = new Mock<IFineRepository>();

    var loan = new Loan
    {
        Id = 1,
        DueDate = DateTime.UtcNow.AddDays(-5),
        ReturnDate = null
    };

    var service = new FineService(fineRepositoryMock.Object);

    await service.CreateFineIfNeededAsync(loan);

    fineRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Fine>()), Times.Never);
}

[Fact]
public async Task CreateFineIfNeededAsync_Should_Not_Create_Fine_When_Returned_On_Time()
{
    var fineRepositoryMock = new Mock<IFineRepository>();

    var loan = new Loan
    {
        Id = 1,
        DueDate = DateTime.UtcNow,
        ReturnDate = DateTime.UtcNow
    };

    var service = new FineService(fineRepositoryMock.Object);

    await service.CreateFineIfNeededAsync(loan);

    fineRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Fine>()), Times.Never);
}

[Fact]
public async Task CreateFineIfNeededAsync_Should_Create_Fine_When_Returned_Late()
{
    var fineRepositoryMock = new Mock<IFineRepository>();

    var dueDate = new DateTime(2026, 6, 22);      // Pazartesi
    var returnDate = new DateTime(2026, 6, 25);   // Perşembe

    var loan = new Loan
    {
        Id = 1,
        DueDate = dueDate,
        ReturnDate = returnDate
    };

    Fine? createdFine = null;

    fineRepositoryMock
        .Setup(repo => repo.AddAsync(It.IsAny<Fine>()))
        .Callback<Fine>(fine => createdFine = fine)
        .Returns(Task.CompletedTask);

    var service = new FineService(fineRepositoryMock.Object);

    await service.CreateFineIfNeededAsync(loan);

    fineRepositoryMock.Verify(repo => repo.AddAsync(It.IsAny<Fine>()), Times.Once);

    Assert.NotNull(createdFine);
    Assert.Equal(1, createdFine.LoanId);
    Assert.Equal(15, createdFine.Amount);
    Assert.False(createdFine.IsPaid);
}
}