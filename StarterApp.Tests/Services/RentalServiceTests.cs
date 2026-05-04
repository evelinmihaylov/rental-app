using Moq;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;
using Xunit;

namespace StarterApp.Tests.Services;

public class RentalServiceTests
{
    private readonly Mock<IRentalRepository> _rentalRepositoryMock = new();
    private readonly Mock<IItemRepository> _itemRepositoryMock = new();
    private readonly Mock<IAuthenticationService> _authenticationServiceMock = new();

    private RentalService CreateSut()
    {
        return new RentalService(
            _rentalRepositoryMock.Object,
            _itemRepositoryMock.Object,
            _authenticationServiceMock.Object);
    }

    private void SetAuthenticatedUser(int userId)
    {
        var user = new User
        {
            Id = userId,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };

        _authenticationServiceMock.SetupGet(x => x.IsAuthenticated).Returns(true);
        _authenticationServiceMock.SetupGet(x => x.CurrentUser).Returns(user);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateRentalAsync_InvalidItemId_ThrowsInvalidOperationException(int itemId)
    {
        // Arrange
        SetAuthenticatedUser(2);
        var sut = CreateSut();

        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(2);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.CreateRentalAsync(itemId, startDate, endDate));

        // Assert
        Assert.Equal("Invalid item selected.", exception.Message);
    }

    [Fact]
    public async Task CreateRentalAsync_StartDateInPast_ThrowsInvalidOperationException()
    {
        // Arrange
        SetAuthenticatedUser(2);
        var sut = CreateSut();

        var startDate = DateTime.Today.AddDays(-1);
        var endDate = DateTime.Today.AddDays(2);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.CreateRentalAsync(1, startDate, endDate));

        // Assert
        Assert.Equal("Start date must be today or later.", exception.Message);
    }

    [Fact]
    public async Task CreateRentalAsync_EndDateNotAfterStartDate_ThrowsInvalidOperationException()
    {
        // Arrange
        SetAuthenticatedUser(2);
        var sut = CreateSut();

        var startDate = DateTime.Today.AddDays(2);
        var endDate = startDate;

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.CreateRentalAsync(1, startDate, endDate));

        // Assert
        Assert.Equal("End date must be after start date.", exception.Message);
    }

    [Fact]
    public async Task CreateRentalAsync_ItemNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        SetAuthenticatedUser(2);
        var sut = CreateSut();

        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);

        _itemRepositoryMock
            .Setup(x => x.GetItemByIdAsync(1))
            .ReturnsAsync((Item?)null);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.CreateRentalAsync(1, startDate, endDate));

        // Assert
        Assert.Equal("Item not found.", exception.Message);
    }

    [Fact]
    public async Task CreateRentalAsync_UnavailableItem_ThrowsInvalidOperationException()
    {
        // Arrange
        SetAuthenticatedUser(2);
        var sut = CreateSut();

        var item = new Item
        {
            Id = 1,
            OwnerId = 1,
            IsAvailable = false,
            Title = "Tent",
            DailyRate = 10
        };

        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);

        _itemRepositoryMock
            .Setup(x => x.GetItemByIdAsync(1))
            .ReturnsAsync(item);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.CreateRentalAsync(1, startDate, endDate));

        // Assert
        Assert.Equal("This item is currently unavailable.", exception.Message);
    }

    [Fact]
    public async Task CreateRentalAsync_UserRentsOwnItem_ThrowsInvalidOperationException()
    {
        // Arrange
        SetAuthenticatedUser(5);
        var sut = CreateSut();

        var item = new Item
        {
            Id = 1,
            OwnerId = 5,
            IsAvailable = true,
            Title = "Drill",
            DailyRate = 8
        };

        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(3);

        _itemRepositoryMock
            .Setup(x => x.GetItemByIdAsync(1))
            .ReturnsAsync(item);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.CreateRentalAsync(1, startDate, endDate));

        // Assert
        Assert.Equal("You cannot rent your own item.", exception.Message);
    }

    [Fact]
    public async Task CreateRentalAsync_ValidRequest_CallsRepositoryAndReturnsRental()
    {
        // Arrange
        SetAuthenticatedUser(2);
        var sut = CreateSut();

        var item = new Item
        {
            Id = 1,
            OwnerId = 1,
            IsAvailable = true,
            Title = "Camera",
            DailyRate = 15
        };

        var startDate = DateTime.Today.AddDays(1);
        var endDate = DateTime.Today.AddDays(4);

        var expectedRental = new Rental
        {
            Id = 10,
            ItemId = 1,
            BorrowerId = 2,
            OwnerId = 1,
            Status = "Requested",
            StartDate = startDate,
            EndDate = endDate
        };

        _itemRepositoryMock
            .Setup(x => x.GetItemByIdAsync(1))
            .ReturnsAsync(item);

        _rentalRepositoryMock
            .Setup(x => x.CreateRentalAsync(1, startDate, endDate))
            .ReturnsAsync(expectedRental);

        // Act
        var result = await sut.CreateRentalAsync(1, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRental.Id, result!.Id);

        _rentalRepositoryMock.Verify(
            x => x.CreateRentalAsync(1, startDate, endDate),
            Times.Once);
    }

    [Fact]
    public async Task ApproveRentalAsync_ValidRequestedRental_UpdatesStatus()
    {
        // Arrange
        SetAuthenticatedUser(1);
        var sut = CreateSut();

        var rental = new Rental
        {
            Id = 7,
            ItemId = 99,
            OwnerId = 1,
            BorrowerId = 2,
            Status = "Requested",
            StartDate = DateTime.Today.AddDays(2),
            EndDate = DateTime.Today.AddDays(5)
        };

        var updatedRental = new Rental
        {
            Id = 7,
            ItemId = 99,
            OwnerId = 1,
            BorrowerId = 2,
            Status = "Approved",
            StartDate = rental.StartDate,
            EndDate = rental.EndDate
        };

        _rentalRepositoryMock
            .Setup(x => x.GetRentalByIdAsync(7))
            .ReturnsAsync(rental);

        _rentalRepositoryMock
            .Setup(x => x.GetIncomingRentalsAsync(It.IsAny<string?>()))
            .ReturnsAsync(new List<Rental>());

        _rentalRepositoryMock
            .Setup(x => x.UpdateRentalStatusAsync(7, "Approved"))
            .ReturnsAsync(updatedRental);

        // Act
        var result = await sut.ApproveRentalAsync(7);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Approved", result!.Status);

        _rentalRepositoryMock.Verify(
            x => x.UpdateRentalStatusAsync(7, "Approved"),
            Times.Once);
    }

    [Fact]
    public async Task ApproveRentalAsync_InvalidTransition_ThrowsInvalidOperationException()
    {
        // Arrange
        SetAuthenticatedUser(1);
        var sut = CreateSut();

        var rental = new Rental
        {
            Id = 7,
            ItemId = 99,
            OwnerId = 1,
            BorrowerId = 2,
            Status = "Completed",
            StartDate = DateTime.Today.AddDays(-5),
            EndDate = DateTime.Today.AddDays(-1)
        };

        _rentalRepositoryMock
            .Setup(x => x.GetRentalByIdAsync(7))
            .ReturnsAsync(rental);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.ApproveRentalAsync(7));

        // Assert
        Assert.Equal("Cannot transition from Completed to Approved.", exception.Message);
    }

    [Fact]
    public async Task ReturnRentalAsync_UserIsNotBorrower_ThrowsInvalidOperationException()
    {
        // Arrange
        SetAuthenticatedUser(1);
        var sut = CreateSut();

        var rental = new Rental
        {
            Id = 12,
            ItemId = 20,
            OwnerId = 1,
            BorrowerId = 5,
            Status = "Out for Rent",
            StartDate = DateTime.Today.AddDays(-2),
            EndDate = DateTime.Today.AddDays(2)
        };

        _rentalRepositoryMock
            .Setup(x => x.GetRentalByIdAsync(12))
            .ReturnsAsync(rental);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.ReturnRentalAsync(12));

        // Assert
        Assert.Equal("Only the borrower can mark this rental as returned.", exception.Message);
    }

    [Fact]
    public async Task MarkOutForRentAsync_BeforeStartDate_ThrowsInvalidOperationException()
    {
        // Arrange
        SetAuthenticatedUser(1);
        var sut = CreateSut();

        var rental = new Rental
        {
            Id = 14,
            ItemId = 25,
            OwnerId = 1,
            BorrowerId = 3,
            Status = "Approved",
            StartDate = DateTime.Today.AddDays(2),
            EndDate = DateTime.Today.AddDays(4)
        };

        _rentalRepositoryMock
            .Setup(x => x.GetRentalByIdAsync(14))
            .ReturnsAsync(rental);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.MarkOutForRentAsync(14));

        // Assert
        Assert.Equal(
            "This rental can only be marked Out for Rent on or after the start date.",
            exception.Message);
    }

    [Fact]
    public async Task ApproveRentalAsync_OverlappingActiveRentalExists_ThrowsInvalidOperationException()
    {
        // Arrange
        SetAuthenticatedUser(1);
        var sut = CreateSut();

        var targetRental = new Rental
        {
            Id = 21,
            ItemId = 50,
            OwnerId = 1,
            BorrowerId = 2,
            Status = "Requested",
            StartDate = DateTime.Today.AddDays(3),
            EndDate = DateTime.Today.AddDays(6)
        };

        var conflictingRental = new Rental
        {
            Id = 22,
            ItemId = 50,
            OwnerId = 1,
            BorrowerId = 7,
            Status = "Approved",
            StartDate = DateTime.Today.AddDays(4),
            EndDate = DateTime.Today.AddDays(7)
        };

        _rentalRepositoryMock
            .Setup(x => x.GetRentalByIdAsync(21))
            .ReturnsAsync(targetRental);

        _rentalRepositoryMock
            .Setup(x => x.GetIncomingRentalsAsync(It.IsAny<string?>()))
            .ReturnsAsync(new List<Rental> { conflictingRental });

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.ApproveRentalAsync(21));

        // Assert
        Assert.Equal(
            "This item already has another active rental for overlapping dates.",
            exception.Message);
    }
}