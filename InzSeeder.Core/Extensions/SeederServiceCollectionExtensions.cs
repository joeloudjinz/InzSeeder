using System.Reflection;
using InzSeeder.Core.Contracts;
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
    /// <param name="assemblies">The assemblies list that contains the seed models</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddInzSeeder(this IServiceCollection services, params Assembly[] assemblies)
    {
        // Register seeder services
        // The data provider is a singleton as it can be shared and caches file content hashes.
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

        return services;
    }

    public static IServiceCollection RegisterEntitySeedersFromAssemblies(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];
        services.Scan(selector => selector.FromAssemblies(assemblies)
            .AddClasses(filter => filter.AssignableTo<IEntitySeeder>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        return services;
    }

    /// <summary>
    /// Adds the seeder services to the service collection with a specific DbContext.
    /// </summary>
    /// <typeparam name="TContext">The type of DbContext.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="optionsAction">The DbContext options configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddInzSeeder<TContext>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
        where TContext : DbContext, ISeederDbContext
    {
        // Register the DbContext
        services.AddDbContext<TContext>(optionsAction);

        // Register seeder services
        services.AddInzSeeder();

        return services;
    }
}