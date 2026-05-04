using Moq;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;
using StarterApp.ViewModels;
using Xunit;

namespace StarterApp.Tests.ViewModels;

public class NearbyItemsViewModelTests
{
    private readonly Mock<IItemRepository> _itemRepositoryMock = new();
    private readonly Mock<ILocationService> _locationServiceMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();

    private NearbyItemsViewModel CreateSut()
    {
        return new NearbyItemsViewModel(
            _itemRepositoryMock.Object,
            _locationServiceMock.Object,
            _navigationServiceMock.Object);
    }

    [Fact]
    public async Task InitializeAsync_NoCategories_LoadsCategoriesAndAddsAllCategoriesOption()
    {
        // Arrange
        var sut = CreateSut();

        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Tools", Slug = "tools" },
            new Category { Id = 2, Name = "Camping", Slug = "camping" }
        };

        _itemRepositoryMock
            .Setup(x => x.GetAllCategoriesAsync())
            .ReturnsAsync(categories);

        // Act
        await sut.InitializeAsync();

        // Assert
        Assert.NotNull(sut.Categories);
        Assert.Equal(3, sut.Categories.Count);
        Assert.Equal("All Categories", sut.Categories[0].Name);
        Assert.Equal("tools", sut.Categories[1].Slug);
        Assert.Equal("camping", sut.Categories[2].Slug);
    }

    [Fact]
    public async Task FindNearbyItemsCommand_InvalidRadius_SetsErrorAndDoesNotCallLocationService()
    {
        // Arrange
        var sut = CreateSut();
        sut.RadiusText = "abc";

        // Act
        await sut.FindNearbyItemsCommand.ExecuteAsync(null);

        // Assert
        Assert.True(sut.HasError);
        Assert.Equal("Radius must be a number.", sut.ErrorMessage);

        _locationServiceMock.Verify(
            x => x.GetCurrentLocationAsync(),
            Times.Never);

        _itemRepositoryMock.Verify(
            x => x.GetNearbyItemsAsync(It.IsAny<double>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<string?>()),
            Times.Never);
    }

    [Fact]
    public async Task FindNearbyItemsCommand_NullLocation_SetsError()
    {
        // Arrange
        var sut = CreateSut();
        sut.RadiusText = "5";

        _locationServiceMock
            .Setup(x => x.GetCurrentLocationAsync())
            .ReturnsAsync((AppLocation?)null);

        // Act
        await sut.FindNearbyItemsCommand.ExecuteAsync(null);

        // Assert
        Assert.True(sut.HasError);
        Assert.Equal(
            "Could not get your current location. Check location permissions and try again.",
            sut.ErrorMessage);
        Assert.False(sut.HasSearched);
        Assert.Empty(sut.NearbyItems);
    }

    [Fact]
    public async Task FindNearbyItemsCommand_ValidInput_LoadsItemsAndUpdatesSummary()
    {
        // Arrange
        var sut = CreateSut();
        sut.RadiusText = "5";

        var location = new AppLocation(55.9533, -3.1883);

        var items = new List<Item>
        {
            new Item
            {
                Id = 1,
                Title = "Tent",
                Latitude = 55.9540,
                Longitude = -3.1890,
                Distance = 2.4,
                DailyRate = 15,
                CategoryId = 2
            },
            new Item
            {
                Id = 2,
                Title = "Stove",
                Latitude = 55.9550,
                Longitude = -3.1900,
                Distance = 3.1,
                DailyRate = 8,
                CategoryId = 2
            }
        };

        _locationServiceMock
            .Setup(x => x.GetCurrentLocationAsync())
            .ReturnsAsync(location);

        _itemRepositoryMock
            .Setup(x => x.GetNearbyItemsAsync(55.9533, -3.1883, 5, null))
            .ReturnsAsync(items);

        // Act
        await sut.FindNearbyItemsCommand.ExecuteAsync(null);

        // Assert
        Assert.False(sut.HasError);
        Assert.True(sut.HasSearched);
        Assert.Equal(2, sut.NearbyItems.Count);
        Assert.Contains("Showing items within 5", sut.LocationSummary);
        Assert.Contains("55.9533", sut.LocationSummary);

        _itemRepositoryMock.Verify(
            x => x.GetNearbyItemsAsync(55.9533, -3.1883, 5, null),
            Times.Once);
    }

    [Fact]
    public async Task FindNearbyItemsCommand_SelectedCategory_PassesSlugToRepository()
    {
        // Arrange
        var sut = CreateSut();
        sut.RadiusText = "5";
        sut.SelectedCategory = new Category
        {
            Id = 2,
            Name = "Camping",
            Slug = "camping"
        };

        var location = new AppLocation(55.9533, -3.1883);

        _locationServiceMock
            .Setup(x => x.GetCurrentLocationAsync())
            .ReturnsAsync(location);

        _itemRepositoryMock
            .Setup(x => x.GetNearbyItemsAsync(55.9533, -3.1883, 5, "camping"))
            .ReturnsAsync(new List<Item>());

        // Act
        await sut.FindNearbyItemsCommand.ExecuteAsync(null);

        // Assert
        _itemRepositoryMock.Verify(
            x => x.GetNearbyItemsAsync(55.9533, -3.1883, 5, "camping"),
            Times.Once);
    }

    [Fact]
    public async Task RefreshCommand_SetsRefreshingBackToFalse_WhenFinished()
    {
        // Arrange
        var sut = CreateSut();
        sut.RadiusText = "5";

        var location = new AppLocation(55.9533, -3.1883);

        _locationServiceMock
            .Setup(x => x.GetCurrentLocationAsync())
            .ReturnsAsync(location);

        _itemRepositoryMock
            .Setup(x => x.GetNearbyItemsAsync(55.9533, -3.1883, 5, null))
            .ReturnsAsync(new List<Item>());

        // Act
        await sut.RefreshCommand.ExecuteAsync(null);

        // Assert
        Assert.False(sut.IsRefreshing);
        Assert.False(sut.IsBusy);
    }

    [Fact]
    public async Task ViewItemCommand_ValidItem_NavigatesToItemDetail()
    {
        // Arrange
        var sut = CreateSut();
        var item = new Item { Id = 4, Title = "Tent" };

        // Act
        await sut.ViewItemCommand.ExecuteAsync(item);

        // Assert
        _navigationServiceMock.Verify(
            x => x.NavigateToAsync(
                "itemdetail",
                It.Is<Dictionary<string, object>>(parameters =>
                    (int)parameters["id"] == 4)),
            Times.Once);
    }
}
