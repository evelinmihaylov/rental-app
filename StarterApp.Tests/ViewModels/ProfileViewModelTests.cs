using Moq;
using StarterApp.Database.Data.Repositories;
using StarterApp.Database.Models;
using StarterApp.Services;
using StarterApp.ViewModels;
using Xunit;

namespace StarterApp.Tests.ViewModels;

public class ProfileViewModelTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock = new();
    private readonly Mock<INavigationService> _navigationServiceMock = new();
    private readonly Mock<IItemRepository> _itemRepositoryMock = new();
    private readonly Mock<IRentalService> _rentalServiceMock = new();
    private readonly Mock<IReviewService> _reviewServiceMock = new();

    private ProfileViewModel CreateSut()
    {
        return new ProfileViewModel(
            _authServiceMock.Object,
            _navigationServiceMock.Object,
            _itemRepositoryMock.Object,
            _rentalServiceMock.Object,
            _reviewServiceMock.Object);
    }

    [Fact]
    public void Constructor_CurrentUserExists_SetsTitleAndCurrentUser()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Eva",
            LastName = "Lopez",
            Email = "eva@example.com"
        };

        _authServiceMock.SetupGet(x => x.CurrentUser).Returns(user);

        // Act
        var sut = CreateSut();

        // Assert
        Assert.Equal("Profile", sut.Title);
        Assert.NotNull(sut.CurrentUser);
        Assert.Equal("eva@example.com", sut.CurrentUser!.Email);
    }

    [Fact]
    public void TextProperties_WithCurrentUser_ReturnFormattedValues()
    {
        // Arrange
        var user = new User
        {
            FirstName = "Eva",
            LastName = "Lopez",
            Email = "eva@example.com",
            AverageRating = 4.25,
            ItemsListed = 7,
            RentalsCompleted = 12,
            CreatedAt = new DateTime(2026, 4, 15)
        };

        _authServiceMock.SetupGet(x => x.CurrentUser).Returns(user);

        // Act
        var sut = CreateSut();

        // Assert
        Assert.Equal("Eva Lopez", sut.FullNameText);
        Assert.Equal("eva@example.com", sut.EmailText);
        Assert.Equal("4.2 / 5", sut.AverageRatingText);
        Assert.Equal("7", sut.ItemsListedText);
        Assert.Equal("12", sut.RentalsCompletedText);
        Assert.Equal("15/04/2026", sut.MemberSinceText);
    }

    [Fact]
    public void TextProperties_WithoutCurrentUser_ReturnFallbackValues()
    {
        // Arrange
        _authServiceMock.SetupGet(x => x.CurrentUser).Returns((User?)null);

        // Act
        var sut = CreateSut();

        // Assert
        Assert.Equal("Unknown user", sut.FullNameText);
        Assert.Equal("Unknown email", sut.EmailText);
        Assert.Equal("No rating yet", sut.AverageRatingText);
        Assert.Equal("0", sut.ItemsListedText);
        Assert.Equal("0", sut.RentalsCompletedText);
        Assert.Equal("Unknown", sut.MemberSinceText);
    }

    [Fact]
    public void CurrentUserChanged_UpdatesDerivedDisplayProperties()
    {
        // Arrange
        _authServiceMock.SetupGet(x => x.CurrentUser).Returns((User?)null);
        var sut = CreateSut();

        var updatedUser = new User
        {
            FirstName = "Maria",
            LastName = "Smith",
            Email = "maria@example.com",
            AverageRating = 5.0,
            ItemsListed = 3,
            RentalsCompleted = 9,
            CreatedAt = new DateTime(2025, 12, 1)
        };

        // Act
        sut.CurrentUser = updatedUser;

        // Assert
        Assert.Equal("Maria Smith", sut.FullNameText);
        Assert.Equal("maria@example.com", sut.EmailText);
        Assert.Equal("5.0 / 5", sut.AverageRatingText);
        Assert.Equal("3", sut.ItemsListedText);
        Assert.Equal("9", sut.RentalsCompletedText);
        Assert.Equal("01/12/2025", sut.MemberSinceText);
    }

    [Fact]
    public async Task ChangePasswordCommand_SetsUnavailableError()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        await sut.ChangePasswordCommand.ExecuteAsync(null);

        // Assert
        Assert.True(sut.HasError);
        Assert.Equal("Password change is not available in this version yet.", sut.ErrorMessage);
    }

    [Fact]
    public void TogglePasswordChangeModeCommand_KeepsModeFalseAndSetsError()
    {
        // Arrange
        var sut = CreateSut();
        sut.IsChangingPassword = true;

        // Act
        sut.TogglePasswordChangeModeCommand.Execute(null);

        // Assert
        Assert.False(sut.IsChangingPassword);
        Assert.True(sut.HasError);
        Assert.Equal("Password change is not available in this version yet.", sut.ErrorMessage);
    }

    [Fact]
    public async Task NavigateBackCommand_CallsNavigationService()
    {
        // Arrange
        var sut = CreateSut();

        _navigationServiceMock
            .Setup(x => x.NavigateBackAsync())
            .Returns(Task.CompletedTask);

        // Act
        await sut.NavigateBackCommand.ExecuteAsync(null);

        // Assert
        _navigationServiceMock.Verify(x => x.NavigateBackAsync(), Times.Once);
    }

    [Fact]
    public async Task InitializeAsync_LoadsFreshStatsFromServices()
    {
        // Arrange
        var user = new User
        {
            Id = 7,
            FirstName = "Eva",
            LastName = "Lopez",
            Email = "eva@example.com"
        };

        _authServiceMock.SetupGet(x => x.CurrentUser).Returns(user);

        _itemRepositoryMock
            .Setup(x => x.GetAllItemsAsync())
            .ReturnsAsync(new List<Item>
            {
                new Item { Id = 1, OwnerId = 7 },
                new Item { Id = 2, OwnerId = 7 },
                new Item { Id = 3, OwnerId = 3 }
            });

        _rentalServiceMock
            .Setup(x => x.GetOutgoingRentalsAsync(null))
            .ReturnsAsync(new List<Rental>
            {
                new Rental { Id = 1, Status = "Completed" },
                new Rental { Id = 2, Status = "Approved" },
                new Rental { Id = 3, Status = "Completed" }
            });

        _reviewServiceMock
            .Setup(x => x.GetUserReviewsAsync(7, 1, 50))
            .ReturnsAsync(new ReviewListResult
            {
                AverageRating = 4.5,
                TotalReviews = 3
            });

        var sut = CreateSut();

        // Act
        await sut.InitializeAsync();

        // Assert
        Assert.Equal("4.5 / 5", sut.AverageRatingText);
        Assert.Equal("2", sut.ItemsListedText);
        Assert.Equal("2", sut.RentalsCompletedText);
        Assert.Equal("3", sut.TotalReviewsText);
    }
}
