using Microsoft.EntityFrameworkCore;
using StarterApp.Database.Data;
using StarterApp.Database.Models;

namespace StarterApp.Tests.Fixtures;

public class DatabaseFixture : IDisposable
{
    public AppDbContext Context { get; }

    public DatabaseFixture()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
        SeedTestData();
    }

    private void SeedTestData()
    {
        var roles = new List<Role>
        {
            new Role
            {
                Id = 1,
                Name = "Admin",
                Description = "Administrator",
                IsDefault = false
            },
            new Role
            {
                Id = 2,
                Name = "User",
                Description = "Standard user",
                IsDefault = true
            }
        };

        var users = new List<User>
        {
            new User
            {
                Id = 1,
                FirstName = "Test",
                LastName = "Admin",
                Email = "admin@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                IsActive = true
            },
            new User
            {
                Id = 2,
                FirstName = "Test",
                LastName = "User",
                Email = "user@example.com",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                IsActive = true
            }
        };

        var userRoles = new List<UserRole>
        {
            new UserRole
            {
                Id = 1,
                UserId = 1,
                RoleId = 1,
                IsActive = true
            },
            new UserRole
            {
                Id = 2,
                UserId = 2,
                RoleId = 2,
                IsActive = true
            }
        };

        Context.Roles.AddRange(roles);
        Context.Users.AddRange(users);
        Context.UserRoles.AddRange(userRoles);
        Context.SaveChanges();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}