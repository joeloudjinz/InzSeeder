using InzSeeder.Core.Algorithms;
using InzSeeder.Core.Services;
using InzSeeder.Core.Tests.Entities;
using InzSeeder.Core.Tests.Factories;
using InzSeeder.Core.Tests.Seeders;
using Microsoft.EntityFrameworkCore;

namespace InzSeeder.Core.Tests.IntegrationTests;

/// <summary>
/// Integration tests for system-owned entity functionality.
/// </summary>
public class SystemOwnedEntityIntegrationTests : IAsyncLifetime
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
    /// Tests that the system-owned flag is correctly set on newly created entities that implement ISystemOwnedEntity.
    /// 
    /// Arrange: Create a seeder for entities that implement ISystemOwnedEntity.
    /// Act: Execute the seeder to create new entities.
    /// Assert: Verify that the IsSystemOwned flag is set to true on newly created entities.
    /// </summary>
    [Fact]
    public async Task SetsSystemOwnedFlagOnNewEntities()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(SystemOwnedEntityIntegrationTests).Assembly);
        var productSeeder = new ProductSeeder();
        
        var serviceProvider = _dbContextWrapper.CreateServiceProviderWithSeeders(
            dataProvider, 
            [productSeeder]);

        // Act
        await EnvironmentSeedingOrchestrator.Orchestrate(serviceProvider, CancellationToken.None);

        // Assert
        var products = await context.Products.ToListAsync();
        Assert.NotEmpty(products);

        // Verify that all products have the IsSystemOwned flag set to true
        foreach (var product in products)
        {
            Assert.True(product.IsSystemOwned);
        }
    }

    /// <summary>
    /// Tests that the system-owned flag is preserved on existing entities when they are updated.
    /// 
    /// Arrange: Pre-populate database with system-owned entities, then run a seeder that updates them.
    /// Act: Execute the seeder to update existing entities.
    /// Assert: Verify that the IsSystemOwned flag remains true on updated entities.
    /// </summary>
    [Fact]
    public async Task PreservesSystemOwnedFlagOnUpdatedEntities()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(SystemOwnedEntityIntegrationTests).Assembly);
        var productSeeder = new ProductSeeder();
        
        // Pre-populate with an existing system-owned product that matches one in our seed data
        var existingProduct = new Product
        {
            Key = "product-1",
            Id = 1,
            Name = "Old Keyboard Name",
            Sku = "LAPTOP-001", // Match the SKU from our seed data
            Price = 59.99m,
            IsSystemOwned = true // Set as system-owned
        };
        context.Products.Add(existingProduct);
        await context.SaveChangesAsync();
        
        var serviceProvider = _dbContextWrapper.CreateServiceProviderWithSeeders(
            dataProvider, 
            [productSeeder]);

        // Act
        await EnvironmentSeedingOrchestrator.Orchestrate(serviceProvider, CancellationToken.None);

        // Assert
        var products = await context.Products.ToListAsync();
        // Should have 2 products: the updated one and the new mouse
        Assert.Equal(2, products.Count);
        
        // Find the updated product (the one with LAPTOP-001 SKU)
        var product = products.FirstOrDefault(p => p.Sku == "LAPTOP-001");
        Assert.NotNull(product);
        
        // Verify that the existing product was updated but still has IsSystemOwned = true
        Assert.Equal("Laptop", product.Name); // Updated from seed data
        Assert.Equal(999.99m, product.Price); // Updated from seed data
        Assert.True(product.IsSystemOwned); // Flag should be preserved
        
        // Verify the new product was added
        var newProduct = products.FirstOrDefault(p => p.Sku == "MOUSE-001");
        Assert.NotNull(newProduct);
        Assert.True(newProduct.IsSystemOwned); // New product should also have flag set
    }
}