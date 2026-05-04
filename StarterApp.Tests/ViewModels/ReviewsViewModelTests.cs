using Moq;
using StarterApp.Database.Models;
using StarterApp.Services;
using StarterApp.ViewModels;
using Xunit;

namespace StarterApp.Tests.ViewModels;

public class ReviewsViewModelTests
{
    private readonly Mock<IReviewService> _reviewServiceMock = new();
    private readonly Mock<IAuthenticationService> _authenticationServiceMock = new();

    private ReviewsViewModel CreateSut()
    {
        _authenticationServiceMock
            .SetupGet(x => x.CurrentUser)
            .Returns(new User { Id = 7 });

        return new ReviewsViewModel(
            _reviewServiceMock.Object,
            _authenticationServiceMock.Object);
    }

    [Fact]
    public async Task InitializeAsync_WithRentalId_LoadsReviewsAndEnablesSubmission()
    {
        // Arrange
        var sut = CreateSut();

        var reviewResult = new ReviewListResult
        {
            Reviews = new List<Review>
            {
                new Review { Id = 1, ItemId = 6, RentalId = 99, Rating = 5, Comment = "Excellent" }
            },
            AverageRating = 5.0,
            TotalReviews = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _reviewServiceMock
            .Setup(x => x.GetItemReviewsAsync(6, 1, 10))
            .ReturnsAsync(reviewResult);

        // Act
        await sut.InitializeAsync(6, 12);

        // Assert
        Assert.Equal(6, sut.ItemId);
        Assert.Equal(12, sut.RentalId);
        Assert.True(sut.CanSubmitReview);
        Assert.Single(sut.Reviews);
        Assert.Equal(5.0, sut.AverageRating);
        Assert.Equal(1, sut.TotalReviews);
    }

    [Fact]
    public async Task InitializeAsync_WithExistingReviewForRental_DisablesSubmission()
    {
        // Arrange
        var sut = CreateSut();

        _reviewServiceMock
            .Setup(x => x.GetItemReviewsAsync(6, 1, 10))
            .ReturnsAsync(new ReviewListResult
            {
                Reviews = new List<Review>
                {
                    new Review
                    {
                        Id = 1,
                        ItemId = 6,
                        RentalId = 12,
                        ReviewerId = 7,
                        Rating = 5,
                        Comment = "Already reviewed"
                    }
                },
                AverageRating = 5.0,
                TotalReviews = 1
            });

        // Act
        await sut.InitializeAsync(6, 12);

        // Assert
        Assert.False(sut.CanSubmitReview);
    }

    [Fact]
    public async Task InitializeAsync_ReviewByAnotherUser_KeepsSubmissionEnabled()
    {
        // Arrange
        var sut = CreateSut();

        _reviewServiceMock
            .Setup(x => x.GetItemReviewsAsync(6, 1, 10))
            .ReturnsAsync(new ReviewListResult
            {
                Reviews = new List<Review>
                {
                    new Review
                    {
                        Id = 1,
                        ItemId = 6,
                        RentalId = 12,
                        ReviewerId = 99,
                        Rating = 5,
                        Comment = "Reviewed by someone else"
                    }
                },
                AverageRating = 5.0,
                TotalReviews = 1
            });

        // Act
        await sut.InitializeAsync(6, 12);

        // Assert
        Assert.True(sut.CanSubmitReview);
    }

    [Fact]
    public async Task InitializeAsync_WithoutRentalId_LoadsReviewsAndDisablesSubmission()
    {
        // Arrange
        var sut = CreateSut();

        _reviewServiceMock
            .Setup(x => x.GetItemReviewsAsync(6, 1, 10))
            .ReturnsAsync(new ReviewListResult
            {
                Reviews = new List<Review>(),
                AverageRating = 0,
                TotalReviews = 0
            });

        // Act
        await sut.InitializeAsync(6, null);

        // Assert
        Assert.Equal(6, sut.ItemId);
        Assert.Null(sut.RentalId);
        Assert.False(sut.CanSubmitReview);
    }

    [Fact]
    public async Task InitializeAsync_InvalidItemId_SetsError()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await sut.InitializeAsync(0, null);

        // Assert
        Assert.True(sut.HasError);
        Assert.Equal("A valid item ID is required.", sut.ErrorMessage);
        Assert.False(sut.IsRefreshing);
    }

    [Fact]
    public async Task SubmitReviewCommand_NoRentalId_SetsError()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await sut.SubmitReviewCommand.ExecuteAsync(null);

        // Assert
        Assert.True(sut.HasError);
        Assert.Equal("This review form is not linked to a completed rental.", sut.ErrorMessage);
    }

    [Fact]
    public async Task SubmitReviewCommand_NoSelectedRating_SetsError()
    {
        // Arrange
        var sut = CreateSut();

        _reviewServiceMock
            .Setup(x => x.GetItemReviewsAsync(6, 1, 10))
            .ReturnsAsync(new ReviewListResult());

        await sut.InitializeAsync(6, 12);

        // Act
        await sut.SubmitReviewCommand.ExecuteAsync(null);

        // Assert
        Assert.True(sut.HasError);
        Assert.Equal("Please select a rating.", sut.ErrorMessage);
    }

    [Fact]
    public async Task SubmitReviewCommand_ValidReview_ClearsFormDisablesSubmitAndReloadsReviews()
    {
        // Arrange
        var sut = CreateSut();

        _reviewServiceMock
            .SetupSequence(x => x.GetItemReviewsAsync(6, 1, 10))
            .ReturnsAsync(new ReviewListResult
            {
                Reviews = new List<Review>(),
                AverageRating = 0,
                TotalReviews = 0
            })
            .ReturnsAsync(new ReviewListResult
            {
                Reviews = new List<Review>
                {
                    new Review
                    {
                        Id = 1,
                        ItemId = 6,
                        RentalId = 12,
                        ReviewerId = 7,
                        Rating = 5,
                        Comment = "Great"
                    }
                },
                AverageRating = 5.0,
                TotalReviews = 1
            });

        _reviewServiceMock
            .Setup(x => x.CreateReviewAsync(12, 5, "Great"))
            .ReturnsAsync(new Review
            {
                Id = 1,
                ItemId = 6,
                RentalId = 12,
                Rating = 5,
                Comment = "Great"
            });

        await sut.InitializeAsync(6, 12);
        sut.SelectedRating = 5;
        sut.Comment = "Great";

        // Act
        await sut.SubmitReviewCommand.ExecuteAsync(null);

        // Assert
        Assert.False(sut.HasError);
        Assert.Null(sut.SelectedRating);
        Assert.Equal(string.Empty, sut.Comment);
        Assert.False(sut.CanSubmitReview);
        Assert.Single(sut.Reviews);
        Assert.Equal(5.0, sut.AverageRating);

        _reviewServiceMock.Verify(x => x.CreateReviewAsync(12, 5, "Great"), Times.Once);
        _reviewServiceMock.Verify(x => x.GetItemReviewsAsync(6, 1, 10), Times.Exactly(2));
    }

    [Fact]
    public async Task SubmitReviewCommand_NullRepositoryResult_SetsError()
    {
        // Arrange
        var sut = CreateSut();

        _reviewServiceMock
            .Setup(x => x.GetItemReviewsAsync(6, 1, 10))
            .ReturnsAsync(new ReviewListResult());

        _reviewServiceMock
            .Setup(x => x.CreateReviewAsync(12, 4, "Good"))
            .ReturnsAsync((Review?)null);

        await sut.InitializeAsync(6, 12);
        sut.SelectedRating = 4;
        sut.Comment = "Good";

        // Act
        await sut.SubmitReviewCommand.ExecuteAsync(null);

        // Assert
        Assert.True(sut.HasError);
        Assert.Equal("Failed to create review.", sut.ErrorMessage);
    }

    [Fact]
    public async Task RefreshCommand_LoadsReviewsAndResetsRefreshing()
    {
        // Arrange
        var sut = CreateSut();

        _reviewServiceMock
            .Setup(x => x.GetItemReviewsAsync(6, 1, 10))
            .ReturnsAsync(new ReviewListResult
            {
                Reviews = new List<Review>(),
                AverageRating = 0,
                TotalReviews = 0
            });

        await sut.InitializeAsync(6, null);

        // Act
        await sut.RefreshCommand.ExecuteAsync(null);

        // Assert
        Assert.False(sut.IsRefreshing);
        Assert.False(sut.IsBusy);

        _reviewServiceMock.Verify(x => x.GetItemReviewsAsync(6, 1, 10), Times.Exactly(2));
    }
}
