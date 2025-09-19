using InzSeeder.Core.Algorithms;
using InzSeeder.Core.Services;
using InzSeeder.Core.Tests.Entities;
using InzSeeder.Core.Tests.Factories;
using InzSeeder.Core.Tests.Seeders;
using Microsoft.EntityFrameworkCore;

namespace InzSeeder.Core.Tests.IntegrationTests;

/// <summary>
/// End-to-end integration tests for the complete seeding workflow.
/// </summary>
public class EndToEndIntegrationTests : IAsyncLifetime
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
    /// Tests the complete seeding workflow with multiple seeders, dependencies, and various configurations.
    /// 
    /// Arrange: Set up a complete test environment with multiple entity types and seeders with dependencies.
    /// Act: Execute the complete seeding workflow.
    /// Assert: Verify that all entities are correctly seeded with proper relationships and flags.
    /// </summary>
    [Fact]
    public async Task ExecutesCompleteSeedingWorkflow()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(EndToEndIntegrationTests).Assembly);
        
        // Create seeders
        var userSeeder = new UserSeeder();
        var userProfileSeeder = new UserProfileSeeder();
        var productSeeder = new ProductSeeder();

        // Create a service provider with all required services and seeders
        var serviceProvider = _dbContextWrapper.CreateServiceProviderWithSeeders(
            dataProvider, 
            [userSeeder, userProfileSeeder, productSeeder]);

        // Act
        await EnvironmentSeedingOrchestrator.Orchestrate(serviceProvider, CancellationToken.None);

        // Assert
        var users = await context.Users.ToListAsync();
        var userProfiles = await context.UserProfiles.ToListAsync();
        var products = await context.Products.ToListAsync();

        // Verify users
        Assert.Equal(2, users.Count);
        Assert.Contains(users, u => u is { Email: "john.doe@example.com", IsSystemOwned: true });
        Assert.Contains(users, u => u is { Email: "jane.smith@example.com", IsSystemOwned: true });

        // Verify user profiles
        Assert.Equal(2, userProfiles.Count);
        Assert.Contains(userProfiles, u => u is { Bio: "John Doe's bio", UserId: 1 });
        Assert.Contains(userProfiles, u => u is { Bio: "Jane Smith's bio", UserId: 2 });

        // Verify products
        Assert.Equal(2, products.Count);
        Assert.Contains(products, p => p is { Sku: "LAPTOP-001", IsSystemOwned: true });
        Assert.Contains(products, p => p is { Sku: "MOUSE-001", IsSystemOwned: true });
    }

    /// <summary>
    /// Tests an incremental seeding workflow where some data already exists.
    /// 
    /// Arrange: Pre-populate database with some existing data, then run a complete seeding workflow.
    /// Act: Execute the seeding workflow.
    /// Assert: Verify that existing data is updated and new data is added correctly.
    /// </summary>
    [Fact]
    public async Task ExecutesIncrementalSeedingWorkflow()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(EndToEndIntegrationTests).Assembly);

        // Pre-populate with some existing data
        var existingUser = new User
        {
            Id = 1,
            Email = "john.doe@example.com",
            Name = "John Doe (Old Name)",
            IsSystemOwned = true
        };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        // Create seeders
        var userSeeder = new UserSeeder();
        var productSeeder = new ProductSeeder();

        // Create a service provider with all required services and seeders
        var serviceProvider = _dbContextWrapper.CreateServiceProviderWithSeeders(
            dataProvider, 
            [userSeeder, productSeeder]);

        // Act
        await EnvironmentSeedingOrchestrator.Orchestrate(serviceProvider, CancellationToken.None);

        // Assert
        // Verify users - should still have 2, with the existing one updated
        var users = await context.Users.ToListAsync();
        Assert.Equal(2, users.Count);

        var johnUser = users.FirstOrDefault(u => u.Email == "john.doe@example.com");
        Assert.NotNull(johnUser);
        Assert.Equal("John Doe", johnUser.Name); // Updated from seed data
        Assert.True(johnUser.IsSystemOwned); // Preserved from existing data

        var janeUser = users.FirstOrDefault(u => u.Email == "jane.smith@example.com");
        Assert.NotNull(janeUser);
        Assert.Equal("Jane Smith", janeUser.Name);
        Assert.True(janeUser.IsSystemOwned);

        // Verify products
        var products = await context.Products.ToListAsync();
        Assert.Equal(2, products.Count);
        Assert.Contains(products, p => p is { Sku: "LAPTOP-001", IsSystemOwned: true });
        Assert.Contains(products, p => p is { Sku: "MOUSE-001", IsSystemOwned: true });
    }
}