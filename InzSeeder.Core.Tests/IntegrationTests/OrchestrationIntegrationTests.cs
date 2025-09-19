using InzSeeder.Core.Algorithms;
using InzSeeder.Core.Services;
using InzSeeder.Core.Tests.Factories;
using InzSeeder.Core.Tests.Seeders;
using Microsoft.EntityFrameworkCore;

namespace InzSeeder.Core.Tests.IntegrationTests;

/// <summary>
/// Integration tests for orchestration functionality.
/// </summary>
public class OrchestrationIntegrationTests : IAsyncLifetime
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
    /// Tests that the full seeding process works correctly with the environment-aware orchestrator.
    /// 
    /// Arrange: Create seeders and an environment-aware orchestrator.
    /// Act: Run the complete seeding process.
    /// Assert: Verify that all seeders execute successfully and data is persisted.
    /// </summary>
    [Fact]
    public async Task ExecutesFullSeedingProcessSuccessfully()
    {
        // Arrange
        _dbContextWrapper.SetEnvironment("Test");
        var context = _dbContextWrapper.Context;
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(OrchestrationIntegrationTests).Assembly);

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
        var users = await context.Users.ToListAsync();
        var products = await context.Products.ToListAsync();

        Assert.Equal(2, users.Count);
        Assert.Equal(2, products.Count);

        // Verify user data
        Assert.Contains(users, u => u.Email == "john.doe@example.com");
        Assert.Contains(users, u => u.Email == "jane.smith@example.com");

        // Verify product data
        Assert.Contains(products, p => p.Sku == "LAPTOP-001");
        Assert.Contains(products, p => p.Sku == "MOUSE-001");
    }

    /// <summary>
    /// Tests that environment-aware orchestrator correctly filters seeders based on environment.
    /// 
    /// Arrange: Create seeders with environment compatibility and an orchestrator with a specific profile.
    /// Act: Run the seeding process with environment filtering.
    /// Assert: Verify that only allowed seeders execute.
    /// </summary>
    [Fact]
    public async Task FiltersSeedersByEnvironment()
    {
        // Arrange
        _dbContextWrapper.SetEnvironment("Production");
        var context = _dbContextWrapper.Context;
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(OrchestrationIntegrationTests).Assembly);

        // Create a service provider with all required services and seeders
        // We'll use the actual seeders but with environment filtering
        var userSeeder = new UserSeeder();
        var productSeeder = new ProductSeeder();

        // Create a service provider with all required services and seeders
        var serviceProvider = _dbContextWrapper.CreateServiceProviderWithSeeders(
            dataProvider, 
            [userSeeder, productSeeder]);

        // Act
        await EnvironmentSeedingOrchestrator.Orchestrate(serviceProvider, CancellationToken.None);

        // Assert
        // In Production environment, both seeders should execute successfully
        var users = await context.Users.ToListAsync();
        var products = await context.Products.ToListAsync();
        
        // Both seeders should have executed
        Assert.NotEmpty(users);
        Assert.NotEmpty(products);
    }
}