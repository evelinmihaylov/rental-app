using System.Net;
using System.Net.Http;
using System.Text;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using Xunit;

namespace StarterApp.Tests.Repositories;

public class ItemRepositoryTests
{
    [Fact]
    public async Task GetNearbyItemsAsync_ValidResponse_ReturnsItems()
    {
        // Arrange
        var json = """
        {
          "items": [
            {
              "id": 1,
              "title": "Tent",
              "description": "2-person hiking tent",
              "dailyRate": 15.0,
              "categoryId": 2,
              "category": "Camping",
              "ownerId": 10,
              "ownerName": "Alice",
              "latitude": 55.9533,
              "longitude": -3.1883,
              "distance": 2.4,
              "isAvailable": true,
              "createdAt": "2026-04-01T10:00:00Z"
            },
            {
              "id": 2,
              "title": "Stove",
              "description": "Portable gas stove",
              "dailyRate": 8.0,
              "categoryId": 2,
              "category": "Camping",
              "ownerId": 11,
              "ownerName": "Bob",
              "latitude": 55.9550,
              "longitude": -3.1900,
              "distance": 3.1,
              "isAvailable": true,
              "createdAt": "2026-04-01T11:00:00Z"
            }
          ],
          "searchLocation": {
            "latitude": 55.9533,
            "longitude": -3.1883
          },
          "radius": 5.0,
          "totalResults": 2
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

        var repository = new ItemRepository(httpClient);

        var testLat = 55.9533;
        var testLon = -3.1883;
        var radiusKm = 5.0;

        // Act
        var items = await repository.GetNearbyItemsAsync(testLat, testLon, radiusKm);

        // Assert
        Assert.NotNull(items);
        Assert.Equal(2, items.Count);
        Assert.All(items, item => Assert.True(item.Distance <= radiusKm));
    }

    [Theory]
    [InlineData("camping", "items/nearby?lat=55.9533&lon=-3.1883&radius=5&category=camping")]
    [InlineData("power tools", "items/nearby?lat=55.9533&lon=-3.1883&radius=5&category=power%20tools")]
    public async Task GetNearbyItemsAsync_WithCategory_AddsCategoryToRequestUrl(
        string category,
        string expectedUrl)
    {
        // Arrange
        string? requestedUrl = null;

        var json = """
        {
          "items": [],
          "searchLocation": {
            "latitude": 55.9533,
            "longitude": -3.1883
          },
          "radius": 5.0,
          "totalResults": 0
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

        var repository = new ItemRepository(httpClient);

        // Act
        await repository.GetNearbyItemsAsync(55.9533, -3.1883, 5, category);

        // Assert
        Assert.Equal(expectedUrl, requestedUrl);
    }

    [Fact]
    public async Task GetNearbyItemsAsync_BadRequest_ReturnsEmptyList()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest));

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var repository = new ItemRepository(httpClient);

        // Act
        var items = await repository.GetNearbyItemsAsync(55.9533, -3.1883, 5);

        // Assert
        Assert.NotNull(items);
        Assert.Empty(items);
    }

    [Fact]
    public async Task GetItemByIdAsync_NotFound_ReturnsNull()
    {
        // Arrange
        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound));

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://example.test/")
        };

        var repository = new ItemRepository(httpClient);

        // Act
        var item = await repository.GetItemByIdAsync(999);

        // Assert
        Assert.Null(item);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ValidResponse_ReturnsCategories()
    {
        // Arrange
        var json = """
        {
          "categories": [
            {
              "id": 1,
              "name": "Tools",
              "slug": "tools"
            },
            {
              "id": 2,
              "name": "Camping",
              "slug": "camping"
            }
          ]
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

        var repository = new ItemRepository(httpClient);

        // Act
        var categories = await repository.GetAllCategoriesAsync();

        // Assert
        Assert.NotNull(categories);
        Assert.Equal(2, categories.Count);
        Assert.Contains(categories, c => c.Slug == "tools");
        Assert.Contains(categories, c => c.Slug == "camping");
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