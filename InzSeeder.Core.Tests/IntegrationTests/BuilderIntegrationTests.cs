using InzSeeder.Core.Contracts;
using InzSeeder.Core.Extensions;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Core.Tests.Data;
using InzSeeder.Core.Tests.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InzSeeder.Core.Tests.IntegrationTests;

/// <summary>
/// Integration tests for builder functionality.
/// </summary>
public class BuilderIntegrationTests : IAsyncLifetime
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
    /// Tests that core InzSeeder services are correctly registered with the DI container.
    /// 
    /// Arrange: Create a service collection and register core InzSeeder services.
    /// Act: Build the service provider and resolve core services.
    /// Assert: Verify that all core services can be resolved successfully.
    /// </summary>
    [Fact]
    public void RegistersCoreServicesWithDependencyInjection()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Add logging services
        services.AddLogging();

        // Add required dependencies for the orchestrator
        services.AddDbContext<TestDbContext>(options =>
            options.UseSqlite("DataSource=:memory:"));
        services.AddScoped<ISeederDbContext>(provider =>
            new Adapters.SeederDbContextAdapter<TestDbContext>(
                provider.GetRequiredService<TestDbContext>()));
        services.AddSingleton<ISeedDataProvider>(new EmbeddedResourceSeedDataProvider(typeof(BuilderIntegrationTests).Assembly));

        // Act
        services.AddInzSeeder();

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        // Verify that core services can be resolved
        Assert.NotNull(serviceProvider.GetService<SeederConfiguration>());
        Assert.NotNull(serviceProvider.GetService<SeedingPerformanceMetricsService>());
        Assert.NotNull(serviceProvider.GetService<SeedingProfileValidationService>());
    }

    /// <summary>
    /// Tests that DbContext adapters are correctly registered with the DI container.
    /// 
    /// Arrange: Create a service collection and register a DbContext with an adapter.
    /// Act: Build the service provider and resolve the adapted DbContext.
    /// Assert: Verify that the adapted DbContext can be resolved successfully.
    /// </summary>
    [Fact]
    public void RegistersDbContextAdapterWithDependencyInjection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Register a DbContext (we'll use our test DbContext)
        services.AddDbContext<TestDbContext>(options =>
            options.UseSqlite("DataSource=:memory:"));

        // Act
        services.AddInzSeeder()
            .UseDbContext<TestDbContext>();

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        // Verify that the adapted DbContext can be resolved
        Assert.NotNull(serviceProvider.GetService<ISeederDbContext>());
    }

    /// <summary>
    /// Tests that entity seeders are correctly registered from assemblies with the DI container.
    /// 
    /// Arrange: Create a service collection and register entity seeders from assemblies.
    /// Act: Build the service provider and resolve registered seeders.
    /// Assert: Verify that seeders can be resolved successfully.
    /// </summary>
    [Fact]
    public void RegistersEntitySeedersFromAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Add logging services
        services.AddLogging();

        // Add required dependencies for seeders
        services.AddDbContext<TestDbContext>(options =>
            options.UseSqlite("DataSource=:memory:"));
        services.AddScoped<ISeederDbContext>(provider =>
            new Adapters.SeederDbContextAdapter<TestDbContext>(
                provider.GetRequiredService<TestDbContext>()));
        services.AddSingleton<ISeedDataProvider>(new EmbeddedResourceSeedDataProvider(typeof(BuilderIntegrationTests).Assembly));

        // Act
        services.AddInzSeeder()
            .RegisterEntitySeedersFromAssemblies(typeof(BuilderIntegrationTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        // Verify that seeders can be resolved
        var seeders = serviceProvider.GetServices<IBaseEntityDataSeeder>().ToList();
        Assert.NotEmpty(seeders);

        // Check that our test seeders are registered
        var seederNames = seeders.Select(s => s.SeedName).ToList();
        Assert.Contains("Users", seederNames);
        Assert.Contains("Products", seederNames);
        // Note: UserProfileSeeder is also registered from the same assembly
        Assert.Contains("UserProfiles", seederNames);
    }

    /// <summary>
    /// Tests that embedded seed data providers are correctly registered with the DI container.
    /// 
    /// Arrange: Create a service collection and register embedded seed data providers.
    /// Act: Build the service provider and resolve the data provider.
    /// Assert: Verify that the data provider can be resolved successfully.
    /// </summary>
    [Fact]
    public void RegistersSeedDataProvidersFromAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddInzSeeder()
            .RegisterEmbeddedSeedDataFromAssemblies(typeof(BuilderIntegrationTests).Assembly);

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        // Verify that the data provider can be resolved
        var dataProvider = serviceProvider.GetService<ISeedDataProvider>();
        Assert.NotNull(dataProvider);
        Assert.IsType<EmbeddedResourceSeedDataProvider>(dataProvider);
    }
}