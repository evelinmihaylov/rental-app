using System.Net;
using System.Net.Http;
using System.Text;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using Xunit;

namespace StarterApp.Tests.Repositories;

public class ReviewRepositoryTests
{
    [Fact]
    public async Task CreateReviewAsync_ValidResponse_ReturnsReview()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;

        var json = """
        {
          "id": 1,
          "rentalId": 12,
          "itemId": 6,
          "itemTitle": "Kayak",
          "reviewerId": 2,
          "reviewerName": "Borrower User",
          "revieweeId": 1,
          "revieweeName": "Owner User",
          "rating": 5,
          "comment": "Excellent item and smooth rental.",
          "createdAt": "2026-05-01T10:00:00Z"
        }
        """;

        var handler = new FakeHttpMessageHandler(request =>
        {
            capturedRequest = request;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var repository = new ReviewRepository(httpClient);

        // Act
        var result = await repository.CreateReviewAsync(12, 5, "Excellent item and smooth rental.");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal(5, result.Rating);
        Assert.Equal("Excellent item and smooth rental.", result.Comment);

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal("reviews", capturedRequest.RequestUri!.PathAndQuery.TrimStart('/'));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest, "Invalid review data.")]
    [InlineData(HttpStatusCode.Conflict, "This rental has already been reviewed.")]
    public async Task CreateReviewAsync_KnownValidationFailure_ThrowsInvalidOperationException(
        HttpStatusCode statusCode,
        string message)
    {
        // Arrange
        var json = $$"""
        {
          "error": "{{statusCode}}",
          "message": "{{message}}",
          "details": null
        }
        """;

        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var repository = new ReviewRepository(httpClient);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.CreateReviewAsync(12, 5, "Great"));

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "You must be logged in to leave a review.")]
    [InlineData(HttpStatusCode.Forbidden, "You do not have permission to review this rental.")]
    public async Task CreateReviewAsync_UnauthorizedOrForbidden_ThrowsUnauthorizedAccessException(
        HttpStatusCode statusCode,
        string message)
    {
        // Arrange
        var json = $$"""
        {
          "error": "{{statusCode}}",
          "message": "{{message}}",
          "details": null
        }
        """;

        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var repository = new ReviewRepository(httpClient);

        // Act
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => repository.CreateReviewAsync(12, 5, "Great"));

        // Assert
        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public async Task GetItemReviewsAsync_ValidResponse_ReturnsPagedReviewResult()
    {
        // Arrange
        var json = """
        {
          "reviews": [
            {
              "id": 1,
              "rentalId": 12,
              "itemId": 6,
              "itemTitle": "Kayak",
              "reviewerId": 2,
              "reviewerName": "Borrower User",
              "revieweeId": 1,
              "revieweeName": "Owner User",
              "rating": 5,
              "comment": "Excellent item and smooth rental.",
              "createdAt": "2026-05-01T10:00:00Z"
            },
            {
              "id": 2,
              "rentalId": 13,
              "itemId": 6,
              "itemTitle": "Kayak",
              "reviewerId": 3,
              "reviewerName": "Second User",
              "revieweeId": 1,
              "revieweeName": "Owner User",
              "rating": 4,
              "comment": "Very good overall.",
              "createdAt": "2026-05-02T10:00:00Z"
            }
          ],
          "averageRating": 4.5,
          "totalReviews": 2,
          "page": 1,
          "pageSize": 10,
          "totalPages": 1
        }
        """;

        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var repository = new ReviewRepository(httpClient);

        // Act
        var result = await repository.GetItemReviewsAsync(6, 1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Reviews.Count);
        Assert.Equal(4.5, result.AverageRating);
        Assert.Equal(2, result.TotalReviews);
        Assert.All(result.Reviews, review => Assert.Equal(6, review.ItemId));
    }

    [Theory]
    [InlineData(6, 1, 10, "items/6/reviews?page=1&pageSize=10")]
    [InlineData(25, 2, 5, "items/25/reviews?page=2&pageSize=5")]
    public async Task GetItemReviewsAsync_BuildsExpectedUrl(
        int itemId,
        int page,
        int pageSize,
        string expectedUrl)
    {
        // Arrange
        string? requestedUrl = null;

        var json = """
        {
          "reviews": [],
          "averageRating": 0,
          "totalReviews": 0,
          "page": 1,
          "pageSize": 10,
          "totalPages": 0
        }
        """;

        var handler = new FakeHttpMessageHandler(request =>
        {
            requestedUrl = request.RequestUri?.PathAndQuery.TrimStart('/');

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var repository = new ReviewRepository(httpClient);

        // Act
        await repository.GetItemReviewsAsync(itemId, page, pageSize);

        // Assert
        Assert.Equal(expectedUrl, requestedUrl);
    }

    [Fact]
    public async Task GetItemReviewsAsync_FailedResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var json = """
        {
          "error": "BadRequest",
          "message": "Failed to load item reviews.",
          "details": null
        }
        """;

        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var repository = new ReviewRepository(httpClient);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.GetItemReviewsAsync(6, 1, 10));

        // Assert
        Assert.Equal("Failed to load item reviews.", exception.Message);
    }

    [Fact]
    public async Task GetUserReviewsAsync_ValidResponse_ReturnsPagedReviewResult()
    {
        // Arrange
        var json = """
        {
          "reviews": [
            {
              "id": 11,
              "rentalId": 12,
              "itemId": 6,
              "itemTitle": "Kayak",
              "reviewerId": 2,
              "reviewerName": "Borrower User",
              "revieweeId": 1,
              "revieweeName": "Owner User",
              "rating": 5,
              "comment": "Excellent item and smooth rental.",
              "createdAt": "2026-05-01T10:00:00Z"
            }
          ],
          "averageRating": 5.0,
          "totalReviews": 1,
          "page": 1,
          "pageSize": 10,
          "totalPages": 1
        }
        """;

        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var repository = new ReviewRepository(httpClient);

        // Act
        var result = await repository.GetUserReviewsAsync(1, 1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Reviews);
        Assert.Equal(5.0, result.AverageRating);
        Assert.All(result.Reviews, review => Assert.Equal(1, review.RevieweeId));
    }

    [Theory]
    [InlineData(1, 1, 10, "users/1/reviews?page=1&pageSize=10")]
    [InlineData(7, 2, 5, "users/7/reviews?page=2&pageSize=5")]
    public async Task GetUserReviewsAsync_BuildsExpectedUrl(
        int userId,
        int page,
        int pageSize,
        string expectedUrl)
    {
        // Arrange
        string? requestedUrl = null;

        var json = """
        {
          "reviews": [],
          "averageRating": 0,
          "totalReviews": 0,
          "page": 1,
          "pageSize": 10,
          "totalPages": 0
        }
        """;

        var handler = new FakeHttpMessageHandler(request =>
        {
            requestedUrl = request.RequestUri?.PathAndQuery.TrimStart('/');

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var repository = new ReviewRepository(httpClient);

        // Act
        await repository.GetUserReviewsAsync(userId, page, pageSize);

        // Assert
        Assert.Equal(expectedUrl, requestedUrl);
    }

    [Fact]
    public async Task GetUserReviewsAsync_FailedResponse_ThrowsInvalidOperationException()
    {
        // Arrange
        var json = """
        {
          "error": "BadRequest",
          "message": "Failed to load user reviews.",
          "details": null
        }
        """;

        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var repository = new ReviewRepository(httpClient);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.GetUserReviewsAsync(1, 1, 10));

        // Assert
        Assert.Equal("Failed to load user reviews.", exception.Message);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}