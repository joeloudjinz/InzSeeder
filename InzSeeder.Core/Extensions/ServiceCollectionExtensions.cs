using System.Reflection;
using InzSeeder.Core.Adapters;
using InzSeeder.Core.Builder;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Orchestrators;
using InzSeeder.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InzSeeder.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core InzSeeder services to the service collection.
    /// This method registers all essential services required for the seeding process
    /// </summary>
    /// <param name="services">The service collection to register the services with.</param>
    /// <param name="configuration">The seeding configuration settings. If null, default settings will be used.</param>
    /// <returns>The InzSeeder builder for method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static ISeederBuilder AddInzSeeder(this IServiceCollection services, SeederConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        
        services.AddSingleton(configuration ?? new SeederConfiguration());
        services.AddScoped<ISeedingOrchestrator, EnvironmentAwareSeedingOrchestrator>();
        services.AddScoped<SeedingPerformanceMetricsService>();
        services.AddScoped<SeedingProfileValidationService>();

        return new SeederBuilder(services);
    }

    /// <summary>
    /// Registers an existing DbContext as an ISeederDbContext adapter.
    /// This method creates an adapter that allows any existing Entity Framework Core DbContext
    /// to be used with the InzSeeder library. The adapter implements the ISeederDbContext interface
    /// which is required by the seeding infrastructure.
    /// This should be called after registering your DbContext with the service collection.
    /// </summary>
    /// <typeparam name="TContext">The type of the existing DbContext that implements DbContext.</typeparam>
    /// <param name="builder">The InzSeeder builder for method chaining.</param>
    /// <returns>The InzSeeder builder for continued method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static ISeederBuilder UseDbContext<TContext>(this ISeederBuilder builder) where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        // Register adapter to convert existing DbContext to ISeederDbContext
        builder.Services.AddScoped<ISeederDbContext>(provider =>
            new DbContextSeederAdapter<TContext>(provider.GetRequiredService<TContext>())
        );

        return builder;
    }

    /// <summary>
    /// Registers entity seeders from the specified assemblies using reflection.
    /// This method scans the provided assemblies for classes that implement the IEntitySeeder interface
    /// and registers them with the dependency injection container with scoped lifetime.
    /// If no assemblies are provided, it will scan the calling assembly by default.
    /// </summary>
    /// <param name="builder">The InzSeeder builder for method chaining.</param>
    /// <param name="assemblies">The assemblies to scan for seeders. If empty, the calling assembly will be used.</param>
    /// <returns>The InzSeeder builder for continued method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static ISeederBuilder RegisterEntitySeedersFromAssemblies(this ISeederBuilder builder, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];
        builder.Services.Scan(selector => selector.FromAssemblies(assemblies)
            .AddClasses(filter => filter.AssignableTo<IEntitySeeder>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        return builder;
    }

    /// <summary>
    /// Registers embedded seed data from the specified assemblies.
    /// This method configures the seeder to load seed data from embedded JSON files
    /// in the specified assemblies. The JSON files should be embedded as resources
    /// and follow the naming convention {SeederName}.json or {SeederName}.{Environment}.json.
    /// The data provider is registered as a singleton as it can be shared across the application
    /// and caches file content hashes for performance optimization.
    /// </summary>
    /// <param name="builder">The InzSeeder builder for method chaining.</param>
    /// <param name="assemblies">The assemblies to load embedded seed data JSON files from. If empty, the calling assembly will be used.</param>
    /// <returns>The InzSeeder builder for continued method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when builder is null.</exception>
    public static ISeederBuilder RegisterEmbeddedSeedDataFromAssemblies(this ISeederBuilder builder, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(builder);
        
        builder.Services.AddSingleton<ISeedDataProvider>(_ => new EmbeddedResourceSeedDataProvider(assemblies));
        return builder;
    }
}