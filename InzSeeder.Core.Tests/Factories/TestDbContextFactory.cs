using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Core.Tests.Data;
using InzSeeder.Core.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Life savor => https://stackoverflow.com/questions/1408175/execute-unit-tests-serially-rather-than-in-parallel
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace InzSeeder.Core.Tests.Factories;

public class TestDbContextWrapper : IAsyncLifetime
{
    private SqliteConnection? _connection;
    private TestDbContext? _context;
    private IServiceProvider? _serviceProvider;

    public TestDbContext Context => _context ?? throw new InvalidOperationException("Context has not been initialized. Call InitializeAsync first.");

    public IServiceProvider ServiceProvider => _serviceProvider ?? throw new InvalidOperationException("ServiceProvider has not been initialized. Call InitializeAsync first.");

    public async Task InitializeAsync()
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
        _connection = new SqliteConnection(connectionStringBuilder.ToString());
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TestDbContext(options);
        await _context.Database.OpenConnectionAsync();
        await _context.Database.EnsureCreatedAsync();

        // Create default service provider with embedded resource data provider
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(TestDbContextWrapper).Assembly);
        _serviceProvider = CreateServiceProviderInternal(dataProvider);
    }

    public void SetEnvironment(string? env)
    {
        EnvironmentUtility.ResetForTesting();
        EnvironmentUtility.DetermineEnvironment(env);
    }

    public async Task DisposeAsync()
    {
        EnvironmentUtility.ResetForTesting();

        if (_context != null)
        {
            await _context.DisposeAsync();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }

    /// <summary>
    /// Creates a service provider with the required services for seeding operations.
    /// </summary>
    /// <param name="dataProvider">The seed data provider.</param>
    /// <param name="seederConfiguration">The seeder configuration (optional).</param>
    /// <returns>A service provider with required services.</returns>
    internal IServiceProvider CreateServiceProvider(ISeedDataProvider dataProvider, SeederConfiguration? seederConfiguration = null)
    {
        return CreateServiceProviderInternal(dataProvider, seederConfiguration);
    }

    /// <summary>
    /// Creates a service provider with all required services and seeders for orchestration.
    /// </summary>
    /// <param name="dataProvider">The seed data provider.</param>
    /// <param name="seeders">The seeders to register.</param>
    /// <param name="seederConfiguration">The seeder configuration (optional).</param>
    /// <returns>A service provider with all required services and seeders.</returns>
    internal IServiceProvider CreateServiceProviderWithSeeders(
        ISeedDataProvider dataProvider,
        IEnumerable<IBaseEntityDataSeeder> seeders,
        SeederConfiguration? seederConfiguration = null
    )
    {
        if (_context == null)
            throw new InvalidOperationException("Context has not been initialized. Call InitializeAsync first.");

        var services = new ServiceCollection();
        services.AddSingleton<ISeederDbContext>(new Adapters.SeederDbContextAdapter<TestDbContext>(_context));
        services.AddSingleton(dataProvider);
        services.AddSingleton(seederConfiguration ?? new SeederConfiguration());
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<SeedingPerformanceMetricsService>(provider => new SeedingPerformanceMetricsService(provider.GetRequiredService<ILogger<SeedingPerformanceMetricsService>>()));

        // Register all seeders
        foreach (var seeder in seeders)
        {
            services.AddSingleton(seeder);
        }

        // Register the validation service
        services.AddSingleton<SeedingProfileValidationService>(provider => new SeedingProfileValidationService(seeders, provider.GetRequiredService<ILogger<SeedingProfileValidationService>>()));

        return services.BuildServiceProvider();
    }

    private IServiceProvider CreateServiceProviderInternal(ISeedDataProvider dataProvider, SeederConfiguration? seederConfiguration = null)
    {
        if (_context == null) throw new InvalidOperationException("Context has not been initialized. Call InitializeAsync first.");

        var services = new ServiceCollection();
        services.AddSingleton<ISeederDbContext>(new Adapters.SeederDbContextAdapter<TestDbContext>(_context));
        services.AddSingleton(dataProvider);
        services.AddSingleton(seederConfiguration ?? new SeederConfiguration());
        services.AddLogging(builder => builder.AddConsole());
        services.AddSingleton<SeedingPerformanceMetricsService>(provider => new SeedingPerformanceMetricsService(provider.GetRequiredService<ILogger<SeedingPerformanceMetricsService>>()));
        return services.BuildServiceProvider();
    }
}