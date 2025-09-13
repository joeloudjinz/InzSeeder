using System.Reflection;
using InzSeeder.Core.Adapters;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Orchestrators;
using InzSeeder.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InzSeeder.Core.Extensions;

public static class SeederServiceCollectionExtensions
{
    /// <summary>
    /// Adds the seeder services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The seeding settings. If null, default settings will be used.</param>
    /// <returns>The InzSeeder builder for chaining.</returns>
    public static ISeederBuilder AddInzSeeder(this IServiceCollection services, SeederConfiguration? configuration = null)
    {
        // Register seeder services
        // The data provider is a singleton as it can be shared and caches file content hashes.
        services.AddSingleton(configuration ?? new SeederConfiguration());
        services.AddSingleton<ISeedDataProvider, EmbeddedResourceSeedDataProvider>();
        services.AddScoped<ISeedingOrchestrator, EnvironmentAwareSeedingOrchestrator>();
        services.AddScoped<SeedingProfileValidationService>();
        services.AddScoped<ExecutionPlanPreviewService>();
        services.AddScoped<SeederHealthCheckService>();
        services.AddScoped<SeedingAuditService>();
        services.AddScoped<SeedDataIntegrityService>();
        services.AddScoped<SeedingMonitoringService>();
        services.AddScoped<SeedingPerformanceMetricsService>();
        services.AddScoped<SeederPurgeService>();

        return new SeederBuilder(services);
    }

    /// <summary>
    /// Registers an existing DbContext as an ISeederDbContext.
    /// This should be called after registering your DbContext with the service collection.
    /// </summary>
    /// <typeparam name="TContext">The type of the existing DbContext.</typeparam>
    /// <param name="builder">The InzSeeder builder.</param>
    /// <returns>The InzSeeder builder for chaining.</returns>
    public static ISeederBuilder UseDbContext<TContext>(this ISeederBuilder builder) where TContext : DbContext
    {
        // Register adapter to convert existing DbContext to ISeederDbContext
        builder.Services.AddScoped<ISeederDbContext>(provider =>
            new DbContextSeederAdapter<TContext>(provider.GetRequiredService<TContext>())
        );

        return builder;
    }

    /// <summary>
    /// Registers entity seeders from the specified assemblies.
    /// </summary>
    /// <param name="builder">The InzSeeder builder.</param>
    /// <param name="assemblies">The assemblies to scan for seeders.</param>
    /// <returns>The InzSeeder builder for chaining.</returns>
    public static ISeederBuilder RegisterEntitySeedersFromAssemblies(this ISeederBuilder builder, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];
        builder.Services.Scan(selector => selector.FromAssemblies(assemblies)
            .AddClasses(filter => filter.AssignableTo<IEntitySeeder>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        return builder;
    }
}