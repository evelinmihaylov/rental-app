using Moq;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;
using Xunit;

namespace StarterApp.Tests.Services;

public class ReviewServiceTests
{
    private readonly Mock<IReviewRepository> _reviewRepositoryMock = new();

    private ReviewService CreateSut()
    {
        return new ReviewService(_reviewRepositoryMock.Object);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateReviewAsync_InvalidRentalId_ThrowsArgumentException(int rentalId)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => sut.CreateReviewAsync(rentalId, 5, "Great item"));

        // Assert
        Assert.Equal("A valid rental ID is required.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public async Task CreateReviewAsync_InvalidRating_ThrowsArgumentException(int rating)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => sut.CreateReviewAsync(12, rating, "Great item"));

        // Assert
        Assert.Equal("Rating must be between 1 and 5.", exception.Message);
    }

    [Fact]
    public async Task CreateReviewAsync_CommentTooLong_ThrowsArgumentException()
    {
        // Arrange
        var sut = CreateSut();
        var comment = new string('a', 501);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => sut.CreateReviewAsync(12, 5, comment));

        // Assert
        Assert.Equal("Comment must be 500 characters or fewer.", exception.Message);
    }

    [Fact]
    public async Task CreateReviewAsync_WhitespaceComment_PassesNullToRepository()
    {
        // Arrange
        var sut = CreateSut();

        var expectedReview = new Review
        {
            Id = 1,
            RentalId = 12,
            Rating = 5,
            Comment = null
        };

        _reviewRepositoryMock
            .Setup(x => x.CreateReviewAsync(12, 5, null))
            .ReturnsAsync(expectedReview);

        // Act
        var result = await sut.CreateReviewAsync(12, 5, "   ");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);

        _reviewRepositoryMock.Verify(
            x => x.CreateReviewAsync(12, 5, null),
            Times.Once);
    }

    [Fact]
    public async Task CreateReviewAsync_ValidComment_TrimmedBeforeCallingRepository()
    {
        // Arrange
        var sut = CreateSut();

        var expectedReview = new Review
        {
            Id = 2,
            RentalId = 12,
            Rating = 4,
            Comment = "Very good"
        };

        _reviewRepositoryMock
            .Setup(x => x.CreateReviewAsync(12, 4, "Very good"))
            .ReturnsAsync(expectedReview);

        // Act
        var result = await sut.CreateReviewAsync(12, 4, "  Very good  ");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Very good", result!.Comment);

        _reviewRepositoryMock.Verify(
            x => x.CreateReviewAsync(12, 4, "Very good"),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task GetItemReviewsAsync_InvalidItemId_ThrowsArgumentException(int itemId)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => sut.GetItemReviewsAsync(itemId, 1, 10));

        // Assert
        Assert.Equal("A valid item ID is required.", exception.Message);
    }

    [Theory]
    [InlineData(0, 10, "Page must be greater than 0.")]
    [InlineData(-1, 10, "Page must be greater than 0.")]
    [InlineData(1, 0, "Page size must be between 1 and 50.")]
    [InlineData(1, 51, "Page size must be between 1 and 50.")]
    public async Task GetItemReviewsAsync_InvalidPaging_ThrowsArgumentException(
        int page,
        int pageSize,
        string expectedMessage)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => sut.GetItemReviewsAsync(6, page, pageSize));

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task GetItemReviewsAsync_ValidInput_ReturnsRepositoryResult()
    {
        // Arrange
        var sut = CreateSut();

        var expectedResult = new ReviewListResult
        {
            Reviews = new List<Review>
            {
                new Review { Id = 1, ItemId = 6, Rating = 5, Comment = "Excellent" }
            },
            AverageRating = 5.0,
            TotalReviews = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _reviewRepositoryMock
            .Setup(x => x.GetItemReviewsAsync(6, 1, 10))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await sut.GetItemReviewsAsync(6, 1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Reviews);
        Assert.Equal(5.0, result.AverageRating);

        _reviewRepositoryMock.Verify(
            x => x.GetItemReviewsAsync(6, 1, 10),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task GetUserReviewsAsync_InvalidUserId_ThrowsArgumentException(int userId)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => sut.GetUserReviewsAsync(userId, 1, 10));

        // Assert
        Assert.Equal("A valid user ID is required.", exception.Message);
    }

    [Theory]
    [InlineData(0, 10, "Page must be greater than 0.")]
    [InlineData(-1, 10, "Page must be greater than 0.")]
    [InlineData(1, 0, "Page size must be between 1 and 50.")]
    [InlineData(1, 100, "Page size must be between 1 and 50.")]
    public async Task GetUserReviewsAsync_InvalidPaging_ThrowsArgumentException(
        int page,
        int pageSize,
        string expectedMessage)
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => sut.GetUserReviewsAsync(3, page, pageSize));

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task GetUserReviewsAsync_ValidInput_ReturnsRepositoryResult()
    {
        // Arrange
        var sut = CreateSut();

        var expectedResult = new ReviewListResult
        {
            Reviews = new List<Review>
            {
                new Review { Id = 10, RevieweeId = 3, Rating = 4, Comment = "Very good lender" }
            },
            AverageRating = 4.0,
            TotalReviews = 1,
            Page = 1,
            PageSize = 10,
            TotalPages = 1
        };

        _reviewRepositoryMock
            .Setup(x => x.GetUserReviewsAsync(3, 1, 10))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await sut.GetUserReviewsAsync(3, 1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Reviews);
        Assert.Equal(4.0, result.AverageRating);

        _reviewRepositoryMock.Verify(
            x => x.GetUserReviewsAsync(3, 1, 10),
            Times.Once);
    }
}