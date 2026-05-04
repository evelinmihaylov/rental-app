using Moq;
using StarterApp.Database.Models;
using StarterApp.Services;
using StarterApp.ViewModels;
using Xunit;

namespace StarterApp.Tests.ViewModels;

public class RentalsViewModelTests
{
    private readonly Mock<IRentalService> _rentalServiceMock = new();
    private readonly Mock<IReviewService> _reviewServiceMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<IAuthenticationService> _authenticationServiceMock = new();

    private RentalsViewModel CreateSut()
    {
        _authenticationServiceMock
            .SetupGet(x => x.CurrentUser)
            .Returns(new User { Id = 7 });

        return new RentalsViewModel(
            _rentalServiceMock.Object,
            _reviewServiceMock.Object,
            _navigationServiceMock.Object,
            _authenticationServiceMock.Object);
    }

    [Fact]
    public async Task InitializeAsync_LoadsIncomingAndOutgoingRentals()
    {
        // Arrange
        var sut = CreateSut();

        var incoming = new List<Rental>
        {
            new Rental { Id = 1, ItemId = 10, Status = "Requested" }
        };

        var outgoing = new List<Rental>
        {
            new Rental { Id = 2, ItemId = 20, Status = "Completed" }
        };

        _rentalServiceMock
            .Setup(x => x.GetIncomingRentalsAsync(null))
            .ReturnsAsync(incoming);

        _rentalServiceMock
            .Setup(x => x.GetOutgoingRentalsAsync(null))
            .ReturnsAsync(outgoing);

        _reviewServiceMock
            .Setup(x => x.GetItemReviewsAsync(20, 1, 50))
            .ReturnsAsync(new ReviewListResult());

        // Act
        await sut.InitializeAsync();

        // Assert
        Assert.Single(sut.IncomingRentals);
        Assert.Single(sut.OutgoingRentals);
        Assert.Equal(1, sut.IncomingRentals[0].Id);
        Assert.Equal(2, sut.OutgoingRentals[0].Id);
        Assert.True(sut.OutgoingRentals[0].CanLeaveReview);
    }

    [Fact]
    public async Task RefreshCommand_LoadsDataAndResetsRefreshing()
    {
        // Arrange
        var sut = CreateSut();

        _rentalServiceMock
            .Setup(x => x.GetIncomingRentalsAsync(null))
            .ReturnsAsync(new List<Rental>());

        _rentalServiceMock
            .Setup(x => x.GetOutgoingRentalsAsync(null))
            .ReturnsAsync(new List<Rental>());

        // Act
        await sut.RefreshCommand.ExecuteAsync(null);

        // Assert
        Assert.False(sut.IsRefreshing);
        Assert.False(sut.IsBusy);

        _rentalServiceMock.Verify(x => x.GetIncomingRentalsAsync(null), Times.Once);
        _rentalServiceMock.Verify(x => x.GetOutgoingRentalsAsync(null), Times.Once);
    }

    [Fact]
    public void ShowOutgoingCommand_SetsOutgoingVisibleAndIncomingHidden()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        sut.ShowOutgoingCommand.Execute(null);

        // Assert
        Assert.False(sut.ShowIncomingRentals);
        Assert.True(sut.ShowOutgoingRentals);
    }

    [Fact]
    public void ShowIncomingCommand_SetsIncomingVisibleAndOutgoingHidden()
    {
        // Arrange
        var sut = CreateSut();
        sut.ShowOutgoingCommand.Execute(null);

        // Act
        sut.ShowIncomingCommand.Execute(null);

        // Assert
        Assert.True(sut.ShowIncomingRentals);
        Assert.False(sut.ShowOutgoingRentals);
    }

    [Fact]
    public async Task ApproveRentalCommand_Success_RefreshesData()
    {
        // Arrange
        var sut = CreateSut();

        var rental = new Rental
        {
            Id = 7,
            ItemId = 100,
            Status = "Requested"
        };

        _rentalServiceMock
            .Setup(x => x.ApproveRentalAsync(7))
            .ReturnsAsync(new Rental
            {
                Id = 7,
                ItemId = 100,
                Status = "Approved"
            });

        _rentalServiceMock
            .Setup(x => x.GetIncomingRentalsAsync(null))
            .ReturnsAsync(new List<Rental>
            {
                new Rental { Id = 7, ItemId = 100, Status = "Approved" }
            });

        _rentalServiceMock
            .Setup(x => x.GetOutgoingRentalsAsync(null))
            .ReturnsAsync(new List<Rental>());

        // Act
        await sut.ApproveRentalCommand.ExecuteAsync(rental);

        // Assert
        Assert.False(sut.HasError);
        Assert.Single(sut.IncomingRentals);
        Assert.Equal("Approved", sut.IncomingRentals[0].Status);

        _rentalServiceMock.Verify(x => x.ApproveRentalAsync(7), Times.Once);
        _rentalServiceMock.Verify(x => x.GetIncomingRentalsAsync(null), Times.Once);
        _rentalServiceMock.Verify(x => x.GetOutgoingRentalsAsync(null), Times.Once);
    }

    [Fact]
    public async Task ApproveRentalCommand_NullUpdatedRental_SetsFallbackError()
    {
        // Arrange
        var sut = CreateSut();

        var rental = new Rental
        {
            Id = 7,
            ItemId = 100,
            Status = "Requested"
        };

        _rentalServiceMock
            .Setup(x => x.ApproveRentalAsync(7))
            .ReturnsAsync((Rental?)null);

        // Act
        await sut.ApproveRentalCommand.ExecuteAsync(rental);

        // Assert
        Assert.True(sut.HasError);
        Assert.Equal("Failed to approve rental.", sut.ErrorMessage);
    }

    [Fact]
    public async Task LeaveReviewCommand_InvalidRentalIds_SetsError()
    {
        // Arrange
        var sut = CreateSut();

        var rental = new Rental
        {
            Id = 0,
            ItemId = 0,
            Status = "Completed"
        };

        // Act
        await sut.LeaveReviewCommand.ExecuteAsync(rental);

        // Assert
        Assert.True(sut.HasError);
        Assert.Equal("A valid completed rental is required.", sut.ErrorMessage);
    }

    [Fact]
    public async Task LeaveReviewCommand_NotCompleted_SetsError()
    {
        // Arrange
        var sut = CreateSut();

        var rental = new Rental
        {
            Id = 8,
            ItemId = 50,
            Status = "Approved"
        };

        // Act
        await sut.LeaveReviewCommand.ExecuteAsync(rental);

        // Assert
        Assert.True(sut.HasError);
        Assert.Equal("Reviews can only be left for completed rentals.", sut.ErrorMessage);
    }

    [Fact]
    public async Task InitializeAsync_CompletedRentalWithExistingReview_HidesLeaveReview()
    {
        // Arrange
        var sut = CreateSut();

        var completedRental = new Rental
        {
            Id = 12,
            ItemId = 30,
            Status = "Completed"
        };

        _rentalServiceMock
            .Setup(x => x.GetIncomingRentalsAsync(null))
            .ReturnsAsync(new List<Rental>());

        _rentalServiceMock
            .Setup(x => x.GetOutgoingRentalsAsync(null))
            .ReturnsAsync(new List<Rental> { completedRental });

        _reviewServiceMock
            .Setup(x => x.GetItemReviewsAsync(30, 1, 50))
            .ReturnsAsync(new ReviewListResult
            {
                Reviews = new List<Review>
                {
                    new Review
                    {
                        Id = 4,
                        ItemId = 30,
                        RentalId = 12,
                        ReviewerId = 7,
                        Rating = 5
                    }
                },
                Page = 1,
                PageSize = 50,
                TotalPages = 1
            });

        // Act
        await sut.InitializeAsync();

        // Assert
        Assert.Single(sut.OutgoingRentals);
        Assert.False(sut.OutgoingRentals[0].CanLeaveReview);
    }

    [Fact]
    public async Task InitializeAsync_CompletedRentalReviewedByAnotherUser_KeepsLeaveReviewVisible()
    {
        // Arrange
        var sut = CreateSut();

        var completedRental = new Rental
        {
            Id = 12,
            ItemId = 30,
            Status = "Completed"
        };

        _rentalServiceMock
            .Setup(x => x.GetIncomingRentalsAsync(null))
            .ReturnsAsync(new List<Rental>());

        _rentalServiceMock
            .Setup(x => x.GetOutgoingRentalsAsync(null))
            .ReturnsAsync(new List<Rental> { completedRental });

        _reviewServiceMock
            .Setup(x => x.GetItemReviewsAsync(30, 1, 50))
            .ReturnsAsync(new ReviewListResult
            {
                Reviews = new List<Review>
                {
                    new Review
                    {
                        Id = 4,
                        ItemId = 30,
                        RentalId = 12,
                        ReviewerId = 99,
                        Rating = 5
                    }
                },
                Page = 1,
                PageSize = 50,
                TotalPages = 1
            });

        // Act
        await sut.InitializeAsync();

        // Assert
        Assert.Single(sut.OutgoingRentals);
        Assert.True(sut.OutgoingRentals[0].CanLeaveReview);
    }

    [Fact]
    public async Task ViewReviewsCommand_ValidRental_NavigatesToReviewsPage()
    {
        // Arrange
        var sut = CreateSut();

        var rental = new Rental
        {
            Id = 8,
            ItemId = 40,
            Status = "Completed"
        };

        _navigationServiceMock
            .Setup(x => x.NavigateToAsync(
                "reviews",
                It.Is<Dictionary<string, object>>(parameters =>
                    (int)parameters["itemId"] == 40)))
            .Returns(Task.CompletedTask);

        // Act
        await sut.ViewReviewsCommand.ExecuteAsync(rental);

        // Assert
        Assert.False(sut.HasError);
        _navigationServiceMock.Verify(x => x.NavigateToAsync(
            "reviews",
            It.Is<Dictionary<string, object>>(parameters =>
                (int)parameters["itemId"] == 40)),
            Times.Once);
    }
}
