using InzSeeder.Core.Adapters;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Orchestrators;
using InzSeeder.Core.Services;
using InzSeeder.Core.Tests.Data;
using InzSeeder.Core.Tests.Factories;
using InzSeeder.Core.Tests.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Tests.IntegrationTests;

/// <summary>
/// Integration tests for environment-specific seed data loading and strict mode functionality.
/// </summary>
public class EnvironmentSpecificAndStrictModeTests : IAsyncLifetime
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
    /// Tests that environment-specific seed data is loaded when available for the current environment.
    /// 
    /// Arrange: Create seed data files for different environments (Development, Production, Staging).
    /// Act: Set the environment and run a seeder that has environment-specific variants.
    /// Assert: Verify that the environment-specific data is loaded instead of the default data.
    /// </summary>
    [Fact]
    public async Task LoadsEnvironmentSpecificSeedDataWhenAvailable()
    {
        // Arrange
        _dbContextWrapper.SetEnvironment("Development");
        var context = _dbContextWrapper.Context;
        var adapter = new SeederDbContextAdapter<TestDbContext>(context);
        var loggerFactory = new LoggerFactory();
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(EnvironmentSpecificAndStrictModeTests).Assembly);

        var seeder = new UserSeeder(dataProvider, adapter, loggerFactory.CreateLogger<UserSeeder>());

        // Act
        await seeder.ExecuteAsync(CancellationToken.None);

        // Assert
        var users = await context.Users.ToListAsync();
        Assert.Equal(2, users.Count);
        
        // Verify that development-specific data was loaded
        Assert.Contains(users, u => u.Email == "dev.john.doe@example.com");
        Assert.Contains(users, u => u.Email == "dev.jane.smith@example.com");
        
        // Verify that default data was NOT loaded
        Assert.DoesNotContain(users, u => u.Email == "john.doe@example.com");
        Assert.DoesNotContain(users, u => u.Email == "jane.smith@example.com");
    }

    /// <summary>
    /// Tests that default seed data is loaded when environment-specific data is not available.
    /// 
    /// Arrange: Set an environment that doesn't have environment-specific seed data.
    /// Act: Run a seeder that has default data but no environment-specific variant for the current environment.
    /// Assert: Verify that the default data is loaded as a fallback.
    /// </summary>
    [Fact]
    public async Task FallsBackToDefaultSeedDataWhenEnvironmentSpecificNotAvailable()
    {
        // Arrange
        _dbContextWrapper.SetEnvironment("NonExistentEnvironment");
        var context = _dbContextWrapper.Context;
        var adapter = new SeederDbContextAdapter<TestDbContext>(context);
        var loggerFactory = new LoggerFactory();
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(EnvironmentSpecificAndStrictModeTests).Assembly);

        var seeder = new UserSeeder(dataProvider, adapter, loggerFactory.CreateLogger<UserSeeder>());

        // Act
        await seeder.ExecuteAsync(CancellationToken.None);

        // Assert
        var users = await context.Users.ToListAsync();
        Assert.Equal(2, users.Count);
        
        // Verify that default data was loaded
        Assert.Contains(users, u => u.Email == "john.doe@example.com");
        Assert.Contains(users, u => u.Email == "jane.smith@example.com");
        
        // Verify that environment-specific data was NOT loaded
        Assert.DoesNotContain(users, u => u.Email == "dev.john.doe@example.com");
        Assert.DoesNotContain(users, u => u.Email == "prod.john.doe@example.com");
    }

    /// <summary>
    /// Tests that the correct environment-specific data is loaded for each environment.
    /// 
    /// Arrange: Create tests for multiple environments with different seed data.
    /// Act: Run seeders in different environments.
    /// Assert: Verify that each environment loads its specific data.
    /// </summary>
    [Fact]
    public async Task LoadsCorrectEnvironmentSpecificDataForDifferentEnvironments()
    {
        // Test Production environment
        await TestEnvironmentSpecificData("Production", 
            ["prod.john.doe@example.com", "prod.jane.smith@example.com", "prod.bob.wilson@example.com"],
            ["john.doe@example.com", "jane.smith@example.com"]);
        
        // Test Staging environment (for Products)
        await TestEnvironmentSpecificProductData("Staging",
            ["Staging Laptop", "Staging Mouse"],
            ["Laptop", "Mouse"]);
    }

    private async Task TestEnvironmentSpecificData(string environment, string[] expectedEmails, string[] unexpectedEmails)
    {
        // Arrange
        var dbContextWrapper = new TestDbContextWrapper();
        dbContextWrapper.SetEnvironment(environment);
        await dbContextWrapper.InitializeAsync();
        
        try
        {
            var context = dbContextWrapper.Context;
            var adapter = new SeederDbContextAdapter<TestDbContext>(context);
            var loggerFactory = new LoggerFactory();
            var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(EnvironmentSpecificAndStrictModeTests).Assembly);

            var seeder = new UserSeeder(dataProvider, adapter, loggerFactory.CreateLogger<UserSeeder>());

            // Act
            await seeder.ExecuteAsync(CancellationToken.None);

            // Assert
            var users = await context.Users.ToListAsync();
            Assert.Equal(expectedEmails.Length, users.Count);
            
            // Verify that environment-specific data was loaded
            foreach (var email in expectedEmails)
            {
                Assert.Contains(users, u => u.Email == email);
            }
            
            // Verify that other environment data was NOT loaded
            foreach (var email in unexpectedEmails)
            {
                Assert.DoesNotContain(users, u => u.Email == email);
            }
        }
        finally
        {
            await dbContextWrapper.DisposeAsync();
        }
    }
    
    private async Task TestEnvironmentSpecificProductData(string environment, string[] expectedNames, string[] unexpectedNames)
    {
        // Arrange
        var dbContextWrapper = new TestDbContextWrapper();
        dbContextWrapper.SetEnvironment(environment);
        await dbContextWrapper.InitializeAsync();
        
        try
        {
            var context = dbContextWrapper.Context;
            var adapter = new SeederDbContextAdapter<TestDbContext>(context);
            var loggerFactory = new LoggerFactory();
            var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(EnvironmentSpecificAndStrictModeTests).Assembly);

            var seeder = new ProductSeeder(dataProvider, adapter, loggerFactory.CreateLogger<ProductSeeder>());

            // Act
            await seeder.ExecuteAsync(CancellationToken.None);

            // Assert
            var products = await context.Products.ToListAsync();
            Assert.Equal(expectedNames.Length, products.Count);
            
            // Verify that environment-specific data was loaded
            foreach (var name in expectedNames)
            {
                Assert.Contains(products, p => p.Name == name);
            }
            
            // Verify that other environment data was NOT loaded
            foreach (var name in unexpectedNames)
            {
                Assert.DoesNotContain(products, p => p.Name == name);
            }
        }
        finally
        {
            await dbContextWrapper.DisposeAsync();
        }
    }

    /// <summary>
    /// Tests that strict mode prevents execution of seeders not explicitly enabled.
    /// 
    /// Arrange: Configure strict mode with only specific seeders enabled.
    /// Act: Run the seeding process with multiple available seeders.
    /// Assert: Verify that only explicitly enabled seeders are executed.
    /// </summary>
    [Fact]
    public async Task StrictModePreventsExecutionOfNonEnabledSeeders()
    {
        // Arrange
        _dbContextWrapper.SetEnvironment("Test");
        var context = _dbContextWrapper.Context;
        var adapter = new SeederDbContextAdapter<TestDbContext>(context);
        var loggerFactory = new LoggerFactory();
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(EnvironmentSpecificAndStrictModeTests).Assembly);

        // Create multiple seeders
        var userSeeder = new UserSeeder(dataProvider, adapter, loggerFactory.CreateLogger<UserSeeder>());
        var productSeeder = new ProductSeeder(dataProvider, adapter, loggerFactory.CreateLogger<ProductSeeder>());
        var userProfileSeeder = new UserProfileSeeder(dataProvider, adapter, loggerFactory.CreateLogger<UserProfileSeeder>());

        var seeders = new List<IEntitySeeder> { userSeeder, productSeeder, userProfileSeeder };

        // Configure strict mode with only Users seeder enabled
        var seederConfiguration = new SeederConfiguration
        {
            Profile = new SeedingProfile
            {
                StrictMode = true,
                EnabledSeeders = ["Users"] // Only enable Users seeder
            }
        };

        // Create required services
        var validationService = new SeedingProfileValidationService(seeders, loggerFactory.CreateLogger<SeedingProfileValidationService>());

        // Create orchestrator
        var orchestrator = new EnvironmentAwareSeedingOrchestrator(
            seeders,
            adapter,
            seederConfiguration,
            validationService,
            loggerFactory.CreateLogger<EnvironmentAwareSeedingOrchestrator>());

        // Act
        await orchestrator.SeedDataAsync(CancellationToken.None);

        // Assert
        // Only Users should be seeded
        var users = await context.Users.ToListAsync();
        var products = await context.Products.ToListAsync();
        var userProfiles = await context.UserProfiles.ToListAsync();

        // Users should be seeded (enabled)
        Assert.NotEmpty(users);
        Assert.Equal(2, users.Count); // Default Users.json data
        
        // Products and UserProfiles should NOT be seeded (not enabled in strict mode)
        Assert.Empty(products);
        Assert.Empty(userProfiles);
    }

    /// <summary>
    /// Tests that strict mode prevents execution of all seeders when none are explicitly enabled.
    /// 
    /// Arrange: Configure strict mode with no explicitly enabled seeders.
    /// Act: Run the seeding process.
    /// Assert: Verify that no seeders are executed (in strict mode, only explicitly enabled seeders will run).
    /// </summary>
    [Fact]
    public async Task StrictModePreventsExecutionOfAllSeedersWhenNoneExplicitlyEnabled()
    {
        // Arrange
        _dbContextWrapper.SetEnvironment("Test");
        var context = _dbContextWrapper.Context;
        var adapter = new SeederDbContextAdapter<TestDbContext>(context);
        var loggerFactory = new LoggerFactory();
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(EnvironmentSpecificAndStrictModeTests).Assembly);

        // Create multiple seeders
        var userSeeder = new UserSeeder(dataProvider, adapter, loggerFactory.CreateLogger<UserSeeder>());
        var productSeeder = new ProductSeeder(dataProvider, adapter, loggerFactory.CreateLogger<ProductSeeder>());

        var seeders = new List<IEntitySeeder> { userSeeder, productSeeder };

        // Configure strict mode with no explicitly enabled seeders
        var seederConfiguration = new SeederConfiguration
        {
            Profile = new SeedingProfile
            {
                StrictMode = true
                // EnabledSeeders is null/empty
            }
        };

        // Create required services
        var validationService = new SeedingProfileValidationService(seeders, loggerFactory.CreateLogger<SeedingProfileValidationService>());

        // Create orchestrator
        var orchestrator = new EnvironmentAwareSeedingOrchestrator(
            seeders,
            adapter,
            seederConfiguration,
            validationService,
            loggerFactory.CreateLogger<EnvironmentAwareSeedingOrchestrator>());

        // Act
        await orchestrator.SeedDataAsync(CancellationToken.None);

        // Assert
        // No seeders should be seeded (in strict mode, only explicitly enabled seeders will run)
        var users = await context.Users.ToListAsync();
        var products = await context.Products.ToListAsync();

        Assert.Empty(users);
        Assert.Empty(products);
    }

    /// <summary>
    /// Tests that strict mode works correctly with environment filtering.
    /// 
    /// Arrange: Configure strict mode with environment-specific seeders.
    /// Act: Run the seeding process in a specific environment.
    /// Assert: Verify that only explicitly enabled seeders for that environment are executed.
    /// </summary>
    [Fact]
    public async Task StrictModeWithEnvironmentFilteringWorksCorrectly()
    {
        // Arrange
        _dbContextWrapper.SetEnvironment("Production");
        var context = _dbContextWrapper.Context;
        var adapter = new SeederDbContextAdapter<TestDbContext>(context);
        var loggerFactory = new LoggerFactory();
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(EnvironmentSpecificAndStrictModeTests).Assembly);

        // Create multiple seeders
        var userSeeder = new UserSeeder(dataProvider, adapter, loggerFactory.CreateLogger<UserSeeder>());
        var productSeeder = new ProductSeeder(dataProvider, adapter, loggerFactory.CreateLogger<ProductSeeder>());

        var seeders = new List<IEntitySeeder> { userSeeder, productSeeder };

        // Configure strict mode with only Products seeder enabled
        var seederConfiguration = new SeederConfiguration
        {
            Profile = new SeedingProfile
            {
                StrictMode = true,
                EnabledSeeders = ["Products"] // Only enable Products seeder
            }
        };

        // Create required services
        var validationService = new SeedingProfileValidationService(seeders, loggerFactory.CreateLogger<SeedingProfileValidationService>());

        // Create orchestrator
        var orchestrator = new EnvironmentAwareSeedingOrchestrator(
            seeders,
            adapter,
            seederConfiguration,
            validationService,
            loggerFactory.CreateLogger<EnvironmentAwareSeedingOrchestrator>());

        // Act
        await orchestrator.SeedDataAsync(CancellationToken.None);

        // Assert
        // Only Products should be seeded with Production-specific data
        var users = await context.Users.ToListAsync();
        var products = await context.Products.ToListAsync();

        // Users should NOT be seeded (not enabled in strict mode)
        Assert.Empty(users);
        
        // Products should be seeded with Production-specific data
        Assert.NotEmpty(products);
        Assert.Equal(3, products.Count); // Enterprise Laptop, Business Smartphone, Enterprise Tablet
        Assert.Contains(products, p => p.Sku == "LAPTOP-001");
        Assert.Contains(products, p => p.Sku == "MOUSE-001");
        Assert.Contains(products, p => p.Sku == "TABLET-001");
    }

    /// <summary>
    /// Tests that strict mode validation correctly identifies invalid seeder references.
    /// 
    /// Arrange: Configure strict mode with references to non-existent seeders.
    /// Act: Attempt to run the seeding process.
    /// Assert: Verify that the validation fails for invalid seeder references.
    /// </summary>
    [Fact]
    public async Task StrictModeValidationDetectsInvalidSeederReferences()
    {
        // Arrange
        _dbContextWrapper.SetEnvironment("Test");
        var context = _dbContextWrapper.Context;
        var adapter = new SeederDbContextAdapter<TestDbContext>(context);
        var loggerFactory = new LoggerFactory();
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(EnvironmentSpecificAndStrictModeTests).Assembly);

        // Create actual seeders
        var userSeeder = new UserSeeder(dataProvider, adapter, loggerFactory.CreateLogger<UserSeeder>());

        var seeders = new List<IEntitySeeder> { userSeeder };

        // Configure strict mode with references to non-existent seeders
        var seederConfiguration = new SeederConfiguration
        {
            Profile = new SeedingProfile
            {
                StrictMode = true,
                EnabledSeeders = ["Users", "NonExistentSeeder"] // Include a non-existent seeder
            }
        };

        // Create required services
        var validationService = new SeedingProfileValidationService(seeders, loggerFactory.CreateLogger<SeedingProfileValidationService>());

        // Create orchestrator
        var orchestrator = new EnvironmentAwareSeedingOrchestrator(
            seeders,
            adapter,
            seederConfiguration,
            validationService,
            loggerFactory.CreateLogger<EnvironmentAwareSeedingOrchestrator>());

        // Act & Assert
        // The validation should fail due to the non-existent seeder reference
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await orchestrator.SeedDataAsync(CancellationToken.None);
        });
    }
}