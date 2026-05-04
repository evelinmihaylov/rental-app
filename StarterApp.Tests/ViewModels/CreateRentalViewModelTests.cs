using Moq;
using StarterApp.Database.Models;
using StarterApp.Services;
using StarterApp.ViewModels;
using Xunit;

namespace StarterApp.Tests.ViewModels;

public class CreateRentalViewModelTests
{
    private readonly Mock<IRentalService> _rentalServiceMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();

    private CreateRentalViewModel CreateSut()
    {
        return new CreateRentalViewModel(
            _rentalServiceMock.Object,
            _navigationServiceMock.Object);
    }

    [Fact]
    public async Task InitializeAsync_SetsItemIdAndDefaultDates()
    {
        // Arrange
        var sut = CreateSut();
        var today = DateTime.Today;

        // Act
        await sut.InitializeAsync(25);

        // Assert
        Assert.Equal(25, sut.ItemId);
        Assert.Equal(today, sut.StartDate);
        Assert.Equal(today.AddDays(1), sut.EndDate);
        Assert.False(sut.HasError);
        Assert.Equal(string.Empty, sut.ErrorMessage);
    }

    [Fact]
    public void Constructor_SetsTitle()
    {
        // Arrange / Act
        var sut = CreateSut();

        // Assert
        Assert.Equal("Request Rental", sut.Title);
    }

    [Fact]
    public async Task SaveAsync_NullResult_SetsError()
    {
        // Arrange
        var sut = CreateSut();
        await sut.InitializeAsync(10);

        _rentalServiceMock
            .Setup(x => x.CreateRentalAsync(10, sut.StartDate, sut.EndDate))
            .ReturnsAsync((Rental?)null);

        // Act
        await sut.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.True(sut.HasError);
        Assert.Equal("Failed to create rental request", sut.ErrorMessage);
        Assert.False(sut.IsBusy);
    }

    [Fact]
    public async Task SaveAsync_ExceptionWithMessage_SetsThatMessage()
    {
        // Arrange
        var sut = CreateSut();
        await sut.InitializeAsync(12);

        _rentalServiceMock
            .Setup(x => x.CreateRentalAsync(12, sut.StartDate, sut.EndDate))
            .ThrowsAsync(new InvalidOperationException("Start date must be today or later."));

        // Act
        await sut.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.True(sut.HasError);
        Assert.Equal("Start date must be today or later.", sut.ErrorMessage);
        Assert.False(sut.IsBusy);
    }

    [Fact]
    public async Task SaveAsync_ExceptionWithoutMessage_SetsFallbackError()
    {
        // Arrange
        var sut = CreateSut();
        await sut.InitializeAsync(15);

        _rentalServiceMock
            .Setup(x => x.CreateRentalAsync(15, sut.StartDate, sut.EndDate))
            .ThrowsAsync(new Exception(string.Empty));

        // Act
        await sut.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.True(sut.HasError);
        Assert.Equal("Failed to save rental request.", sut.ErrorMessage);
        Assert.False(sut.IsBusy);
    }

    [Fact]
    public async Task SaveAsync_AlwaysResetsIsBusy_WhenServiceThrows()
    {
        // Arrange
        var sut = CreateSut();
        await sut.InitializeAsync(30);

        _rentalServiceMock
            .Setup(x => x.CreateRentalAsync(30, sut.StartDate, sut.EndDate))
            .ThrowsAsync(new InvalidOperationException("Invalid rental request."));

        // Act
        await sut.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.False(sut.IsBusy);
    }

    [Fact]
    public async Task SaveAsync_UsesCurrentItemIdAndDates()
    {
        // Arrange
        var sut = CreateSut();
        var startDate = DateTime.Today.AddDays(2);
        var endDate = DateTime.Today.AddDays(5);

        sut.ItemId = 42;
        sut.StartDate = startDate;
        sut.EndDate = endDate;

        _rentalServiceMock
            .Setup(x => x.CreateRentalAsync(42, startDate, endDate))
            .ReturnsAsync((Rental?)null);

        // Act
        await sut.SaveCommand.ExecuteAsync(null);

        // Assert
        _rentalServiceMock.Verify(
            x => x.CreateRentalAsync(42, startDate, endDate),
            Times.Once);
    }

    [Fact]
    public async Task SaveAsync_ValidRental_NavigatesBack()
    {
        // Arrange
        var sut = CreateSut();
        await sut.InitializeAsync(9);

        _rentalServiceMock
            .Setup(x => x.CreateRentalAsync(9, sut.StartDate, sut.EndDate))
            .ReturnsAsync(new Rental { Id = 1, ItemId = 9 });

        // Act
        await sut.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.False(sut.HasError);
        _navigationServiceMock.Verify(x => x.NavigateBackAsync(), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_NavigatesBack()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await sut.CancelCommand.ExecuteAsync(null);

        // Assert
        _navigationServiceMock.Verify(x => x.NavigateBackAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_ClearsPreviousError()
    {
        // Arrange
        var sut = CreateSut();

        _rentalServiceMock
            .Setup(x => x.CreateRentalAsync(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new InvalidOperationException("Something went wrong."));

        await sut.InitializeAsync(5);
        await sut.SaveCommand.ExecuteAsync(null);

        Assert.True(sut.HasError);

        // Act
        await sut.InitializeAsync(6);

        // Assert
        Assert.False(sut.HasError);
        Assert.Equal(string.Empty, sut.ErrorMessage);
        Assert.Equal(6, sut.ItemId);
    }
}
