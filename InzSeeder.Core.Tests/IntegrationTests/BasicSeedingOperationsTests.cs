using InzSeeder.Core.Algorithms;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Tests.Entities;
using InzSeeder.Core.Tests.Factories;
using InzSeeder.Core.Tests.Seeders;
using Microsoft.EntityFrameworkCore;

namespace InzSeeder.Core.Tests.IntegrationTests;

/// <summary>
/// Integration tests for basic seeding operations functionality.
/// </summary>
public class BasicSeedingOperationsTests : IAsyncLifetime
{
    private TestDbContextWrapper _dbContextWrapper = null!;

    public async Task InitializeAsync()
    {
        _dbContextWrapper = new TestDbContextWrapper();
        _dbContextWrapper.SetEnvironment("IntegrationTest");
        await _dbContextWrapper.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContextWrapper.DisposeAsync();
    }

    /// <summary>
    /// Tests that new entities are correctly seeded into an empty database.
    /// 
    /// Arrange: Create an empty in-memory database and a user seeder with embedded resource data.
    /// Act: Execute the seeder to add users from the seed data.
    /// Assert: Verify that all users from the seed data were added to the database.
    /// </summary>
    [Fact]
    public async Task SeedsNewEntitiesCorrectly()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var seeder = new UserSeeder();

        // Act
        await SeederExecutor.Execute(seeder, _dbContextWrapper.ServiceProvider, CancellationToken.None);

        // Assert
        var users = await context.Users.ToListAsync();
        Assert.Equal(2, users.Count);
        Assert.Contains(users, u => u.Email == "john.doe@example.com");
        Assert.Contains(users, u => u.Email == "jane.smith@example.com");
    }

    /// <summary>
    /// Tests that existing entities are updated when re-seeding with matching business keys.
    /// 
    /// Arrange: Pre-populate database with a user that matches seed data by business key (email).
    /// Act: Execute the seeder which should update the existing user and add new ones.
    /// Assert: Verify that the existing user was updated and new users were added.
    /// </summary>
    [Fact]
    public async Task UpdatesExistingEntities()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        //
        var seeder = new UserSeeder();

        // Pre-populate with existing user that matches seed data
        var existingUser = new User
        {
            Key = "user-1",
            Id = 1,
            Email = "john.doe@example.com",
            Name = "John Doe Original"
        };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        // Act
        await SeederExecutor.Execute(seeder, _dbContextWrapper.ServiceProvider, CancellationToken.None);

        // Assert
        var users = await context.Users.ToListAsync();
        Assert.Equal(2, users.Count); // Should have 2 users: john.doe (updated) and jane.smith (new)
        var johnUser = users.FirstOrDefault(u => u.Email == "john.doe@example.com");
        Assert.NotNull(johnUser);
        Assert.Equal("John Doe", johnUser.Name); // Updated name from seed data
        var janeUser = users.FirstOrDefault(u => u.Email == "jane.smith@example.com");
        Assert.NotNull(janeUser);
        Assert.Equal("Jane Smith", janeUser.Name);
    }

    /// <summary>
    /// Tests that seeding is idempotent - running the same seeder twice with unchanged data
    /// should not modify the database on the second run.
    /// 
    /// Arrange: Create a seeder and execute it twice.
    /// Act: Run the seeder twice in succession.
    /// Assert: Verify that only one SeedHistory record exists, indicating the second run was skipped.
    /// </summary>
    [Fact]
    public async Task IsIdempotentWithUnchangedData()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        //
        var seeder = new UserSeeder();

        // Act - Run seeder twice
        await SeederExecutor.Execute(seeder, _dbContextWrapper.ServiceProvider, CancellationToken.None);
        await SeederExecutor.Execute(seeder, _dbContextWrapper.ServiceProvider, CancellationToken.None);

        // Assert
        var users = await context.Users.ToListAsync();
        Assert.Equal(2, users.Count);
        var seedHistory = await context.Set<SeedHistory>().ToListAsync();
        Assert.Single(seedHistory);
    }

    /// <summary>
    /// Tests that entities are re-seeded when the seed data content changes.
    /// 
    /// Arrange: Run a seeder, then simulate data modification by using a custom data provider with different content.
    /// Act: Run the seeder again with modified data.
    /// Assert: Verify that re-seeding occurs when content changes.
    /// </summary>
    [Fact]
    public async Task ReseedsWhenDataContentChanges()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var seeder = new UserSeeder();

        // First run
        await SeederExecutor.Execute(seeder, _dbContextWrapper.ServiceProvider, CancellationToken.None);

        // Assert initial state
        var users = await context.Users.ToListAsync();
        Assert.Equal(2, users.Count);
        Assert.Contains(users, u => u is { Email: "john.doe@example.com", Name: "John Doe" });
        Assert.Contains(users, u => u is { Email: "jane.smith@example.com", Name: "Jane Smith" });

        // Check seed history
        var seedHistory = await context.Set<SeedHistory>().ToListAsync();
        Assert.Single(seedHistory);
        var originalHash = seedHistory[0].ContentHash;

        // Create a custom data provider with modified content
        var modifiedDataProvider = new ModifiedContentSeedDataProvider();

        // Create a new service provider with the modified data provider
        var modifiedServiceProvider = _dbContextWrapper.CreateServiceProvider(modifiedDataProvider);

        // Act - Run with modified data
        await SeederExecutor.Execute(seeder, modifiedServiceProvider, CancellationToken.None);

        // Assert - Verify that re-seeding occurred with new data
        var updatedUsers = await context.Users.ToListAsync();
        Assert.Equal(3, updatedUsers.Count); // Should now have 3 users (2 from modified data + 1 new)
        
        // Verify that existing user was updated with new name
        var updatedJohn = updatedUsers.FirstOrDefault(u => u.Email == "john.doe@example.com");
        Assert.NotNull(updatedJohn);
        Assert.Equal("John Doe Modified", updatedJohn.Name); // Updated name from modified data
        
        // Verify new users from modified data
        Assert.Contains(updatedUsers, u => u is { Email: "jane.smith@example.com", Name: "Jane Smith Modified" });
        Assert.Contains(updatedUsers, u => u is { Email: "bob.wilson@example.com", Name: "Bob Wilson" });
        
        // Verify that seed history was updated with new hash
        var updatedSeedHistory = await context.Set<SeedHistory>().ToListAsync();
        Assert.Single(updatedSeedHistory);
        Assert.NotEqual(originalHash, updatedSeedHistory[0].ContentHash); // Hash should have changed
        // Applied date should be newer or equal (allowing for same timestamp due to fast execution)
        Assert.True(updatedSeedHistory[0].AppliedDateUtc >= seedHistory[0].AppliedDateUtc);
    }

    // Custom data provider that provides modified content
    private class ModifiedContentSeedDataProvider : ISeedDataProvider
    {
        public Task<(string? content, string? hash)> GetSeedDataAsync(string seedName, CancellationToken cancellationToken)
        {
            if (seedName != "Users")
                return Task.FromResult<(string? content, string? hash)>((null, null));

            // Modified content with updated names and an additional user
            var modifiedUsers = new List<UserSeedModel>
            {
                new() { Key = "user-1", Id = 1, Email = "john.doe@example.com", Name = "John Doe Modified" }, // Modified name
                new() { Key = "user-2", Id = 2, Email = "jane.smith@example.com", Name = "Jane Smith Modified" }, // Modified name
                new() { Key = "user-3", Id = 3, Email = "bob.wilson@example.com", Name = "Bob Wilson" } // New user
            };

            var json = System.Text.Json.JsonSerializer.Serialize(modifiedUsers);
            var hash = ComputeHash(json);
            return Task.FromResult<(string? content, string? hash)>((json, hash));
        }

        private static string ComputeHash(string content)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }

    /// <summary>
    /// Tests that seeders handle gracefully when no seed data is found for a given seed name.
    /// 
    /// Arrange: Create a seeder with a seed name that doesn't correspond to any embedded resource.
    /// Act: Execute the seeder.
    /// Assert: Verify that no entities were added to the database.
    /// </summary>
    [Fact]
    public async Task HandlesMissingSeedDataGracefully()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var seeder = new TestUserSeeder("NonExistentSeed");

        // Act
        await SeederExecutor.Execute(seeder, _dbContextWrapper.ServiceProvider, CancellationToken.None);

        // Assert
        var users = await context.Users.ToListAsync();
        Assert.Empty(users);
    }

    // Test-specific seeder that allows us to override the seed name
    private class TestUserSeeder(string seedName) : UserSeeder
    {
        public override string SeedName => seedName;
    }
}