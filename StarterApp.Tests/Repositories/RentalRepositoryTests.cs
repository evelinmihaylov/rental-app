using System.Net;
using System.Net.Http;
using System.Text;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using Xunit;

namespace StarterApp.Tests.Repositories;

public class RentalRepositoryTests
{
    [Fact]
    public async Task CreateRentalAsync_ValidResponse_ReturnsRental()
    {
        // Arrange
        var json = """
        {
          "id": 10,
          "itemId": 5,
          "itemTitle": "Tent",
          "borrowerId": 2,
          "borrowerName": "Test User",
          "ownerId": 1,
          "ownerName": "Owner User",
          "startDate": "2026-05-10T00:00:00Z",
          "endDate": "2026-05-12T00:00:00Z",
          "status": "Requested",
          "totalPrice": 30.0,
          "createdAt": "2026-05-01T10:00:00Z"
        }
        """;

        HttpRequestMessage? capturedRequest = null;

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

        var repository = new RentalRepository(httpClient);

        var startDate = new DateTime(2026, 5, 10);
        var endDate = new DateTime(2026, 5, 12);

        // Act
        var result = await repository.CreateRentalAsync(5, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(10, result!.Id);
        Assert.Equal("Requested", result.Status);
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal("rentals", capturedRequest.RequestUri!.PathAndQuery.TrimStart('/'));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task CreateRentalAsync_KnownFailureStatus_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(statusCode));

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var repository = new RentalRepository(httpClient);

        // Act
        var result = await repository.CreateRentalAsync(
            5,
            new DateTime(2026, 5, 10),
            new DateTime(2026, 5, 12));

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(null, "rentals/incoming")]
    [InlineData("Requested", "rentals/incoming?status=Requested")]
    [InlineData("Out for Rent", "rentals/incoming?status=Out%20for%20Rent")]
    public async Task GetIncomingRentalsAsync_BuildsExpectedUrl(string? status, string expectedUrl)
    {
        // Arrange
        string? requestedUrl = null;

        var json = """
        {
          "rentals": [],
          "totalRentals": 0
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

        var repository = new RentalRepository(httpClient);

        // Act
        await repository.GetIncomingRentalsAsync(status);

        // Assert
        Assert.Equal(expectedUrl, requestedUrl);
    }

    [Fact]
    public async Task GetOutgoingRentalsAsync_ValidResponse_ReturnsRentals()
    {
        // Arrange
        var json = """
        {
          "rentals": [
            {
              "id": 1,
              "itemId": 20,
              "itemTitle": "Camera",
              "borrowerId": 2,
              "borrowerName": "Borrower",
              "ownerId": 1,
              "ownerName": "Owner",
              "startDate": "2026-05-10T00:00:00Z",
              "endDate": "2026-05-11T00:00:00Z",
              "status": "Completed",
              "totalPrice": 15.0,
              "createdAt": "2026-05-01T10:00:00Z"
            }
          ],
          "totalRentals": 1
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

        var repository = new RentalRepository(httpClient);

        // Act
        var rentals = await repository.GetOutgoingRentalsAsync();

        // Assert
        Assert.NotNull(rentals);
        Assert.Single(rentals);
        Assert.Equal("Completed", rentals[0].Status);
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task GetRentalByIdAsync_NotAccessible_ReturnsNull(HttpStatusCode statusCode)
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(statusCode));

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var repository = new RentalRepository(httpClient);

        // Act
        var rental = await repository.GetRentalByIdAsync(999);

        // Assert
        Assert.Null(rental);
    }

    [Fact]
    public async Task UpdateRentalStatusAsync_ValidResponse_ReturnsUpdatedRental()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;

        var json = """
        {
          "id": 12,
          "itemId": 6,
          "itemTitle": "Kayak",
          "borrowerId": 2,
          "borrowerName": "Borrower",
          "ownerId": 1,
          "ownerName": "Owner",
          "startDate": "2026-05-10T00:00:00Z",
          "endDate": "2026-05-12T00:00:00Z",
          "status": "Approved",
          "totalPrice": 50.0,
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

        var repository = new RentalRepository(httpClient);

        // Act
        var result = await repository.UpdateRentalStatusAsync(12, "Approved");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Approved", result!.Status);
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Patch, capturedRequest!.Method);
        Assert.Equal("rentals/12/status", capturedRequest.RequestUri!.PathAndQuery.TrimStart('/'));
    }

    [Fact]
    public async Task UpdateRentalStatusAsync_Forbidden_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var json = """
        {
          "error": "Forbidden",
          "message": "You do not have permission to update this rental."
        }
        """;

        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.Forbidden)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var repository = new RentalRepository(httpClient);

        // Act
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => repository.UpdateRentalStatusAsync(12, "Approved"));

        // Assert
        Assert.Equal("You do not have permission to update this rental.", exception.Message);
    }

    [Fact]
    public async Task UpdateRentalStatusAsync_BadRequest_ThrowsInvalidOperationException()
    {
        // Arrange
        var json = """
        {
          "error": "BadRequest",
          "message": "Invalid rental status transition."
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

        var repository = new RentalRepository(httpClient);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => repository.UpdateRentalStatusAsync(12, "Approved"));

        // Assert
        Assert.Equal("Invalid rental status transition.", exception.Message);
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