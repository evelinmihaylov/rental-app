using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Models;
using StarterApp.Services;
using StarterApp.Tests.Fixtures;
using Xunit;

namespace StarterApp.Tests.Services;

public class LocalAuthenticationServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public LocalAuthenticationServiceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.Context.ChangeTracker.Clear();
    }

    private LocalAuthenticationService CreateSut()
    {
        _fixture.Context.ChangeTracker.Clear();
        return new LocalAuthenticationService(_fixture.Context);
    }

    private async Task<User> CreateUserAsync(
        string email,
        string password,
        params string[] roleNames)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var hash = BCrypt.Net.BCrypt.HashPassword(password, salt);

        var user = new User
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _fixture.Context.Users.Add(user);
        await _fixture.Context.SaveChangesAsync();

        foreach (var roleName in roleNames)
        {
            var role = await _fixture.Context.Roles.FirstAsync(r => r.Name == roleName);
            _fixture.Context.UserRoles.Add(new UserRole(user.Id, role.Id));
        }

        await _fixture.Context.SaveChangesAsync();
        _fixture.Context.ChangeTracker.Clear();

        return user;
    }

    [Fact]
    public async Task RegisterAsync_NewUser_CreatesUserAndAssignsDefaultRole()
    {
        // Arrange
        var sut = CreateSut();
        var email = $"newuser-{Guid.NewGuid():N}@example.com";

        // Act
        var result = await sut.RegisterAsync("New", "User", email, "Password123!");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Registration successful", result.Message);

        var createdUser = await _fixture.Context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        Assert.NotNull(createdUser);
        Assert.Equal("New", createdUser!.FirstName);
        Assert.Contains(createdUser.UserRoles, ur => ur.Role.IsDefault);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = await sut.RegisterAsync("Another", "User", "user@example.com", "Password123!");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User with this email already exists", result.Message);

        var matchingUsers = await _fixture.Context.Users
            .CountAsync(u => u.Email == "user@example.com");

        Assert.Equal(1, matchingUsers);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_SetsCurrentUserAndRoles()
    {
        // Arrange
        var email = $"login-{Guid.NewGuid():N}@example.com";
        await CreateUserAsync(email, "Password123!", "User");

        var sut = CreateSut();

        // Act
        var result = await sut.LoginAsync(email, "Password123!");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Login successful", result.Message);
        Assert.True(sut.IsAuthenticated);
        Assert.NotNull(sut.CurrentUser);
        Assert.Equal(email, sut.CurrentUser!.Email);
        Assert.Contains("User", sut.CurrentUserRoles);
        Assert.True(sut.HasRole("user"));
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsFailure()
    {
        // Arrange
        var email = $"wrongpass-{Guid.NewGuid():N}@example.com";
        await CreateUserAsync(email, "Password123!", "User");

        var sut = CreateSut();

        // Act
        var result = await sut.LoginAsync(email, "WrongPassword!");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Invalid email or password", result.Message);
        Assert.False(sut.IsAuthenticated);
        Assert.Null(sut.CurrentUser);
        Assert.Empty(sut.CurrentUserRoles);
    }

    [Fact]
    public async Task LogoutAsync_AfterSuccessfulLogin_ClearsCurrentUserAndRoles()
    {
        // Arrange
        var email = $"logout-{Guid.NewGuid():N}@example.com";
        await CreateUserAsync(email, "Password123!", "User");

        var sut = CreateSut();
        await sut.LoginAsync(email, "Password123!");

        // Act
        await sut.LogoutAsync();

        // Assert
        Assert.False(sut.IsAuthenticated);
        Assert.Null(sut.CurrentUser);
        Assert.Empty(sut.CurrentUserRoles);
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidCurrentPassword_UpdatesStoredPassword()
    {
        // Arrange
        var email = $"changepass-{Guid.NewGuid():N}@example.com";
        await CreateUserAsync(email, "OldPassword123!", "User");

        var sut = CreateSut();
        await sut.LoginAsync(email, "OldPassword123!");

        // Act
        var changed = await sut.ChangePasswordAsync("OldPassword123!", "NewPassword123!");

        // Assert
        Assert.True(changed);

        await sut.LogoutAsync();

        var oldLogin = await sut.LoginAsync(email, "OldPassword123!");
        Assert.False(oldLogin.IsSuccess);

        var newLogin = await sut.LoginAsync(email, "NewPassword123!");
        Assert.True(newLogin.IsSuccess);
    }

    [Fact]
    public async Task RoleChecks_UserWithMultipleRoles_ReturnsExpectedValues()
    {
        // Arrange
        var email = $"roles-{Guid.NewGuid():N}@example.com";
        await CreateUserAsync(email, "Password123!", "Admin", "User");

        var sut = CreateSut();
        await sut.LoginAsync(email, "Password123!");

        // Act
        var hasAdmin = sut.HasRole("admin");
        var hasAny = sut.HasAnyRole("Guest", "Admin");
        var hasAll = sut.HasAllRoles("Admin", "User");
        var hasAllWithMissingRole = sut.HasAllRoles("Admin", "Guest");

        // Assert
        Assert.True(hasAdmin);
        Assert.True(hasAny);
        Assert.True(hasAll);
        Assert.False(hasAllWithMissingRole);
    }
}