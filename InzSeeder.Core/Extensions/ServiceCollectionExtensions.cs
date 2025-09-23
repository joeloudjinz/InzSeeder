using System.Reflection;
using InzSeeder.Core.Adapters;
using InzSeeder.Core.Builder;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the core InzSeeder services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The <see cref="SeederConfiguration"/> to use. If null, default settings will be applied.</param>
    /// <returns>An <see cref="ISeederBuilder"/> for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static ISeederBuilder AddInzSeeder(this IServiceCollection services, SeederConfiguration? configuration = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddLogging(options => { options.AddConsole(); });
        services.AddSingleton(configuration ?? new SeederConfiguration());
        services.AddScoped<IEntityReferenceResolver, EntityReferenceResolver>();
        services.AddScoped<SeedingPerformanceMetricsService>();
        services.AddScoped<SeedingProfileValidationService>();

        return new SeederBuilder(services);
    }

    /// <summary>
    /// Configures the seeder to use the specified <see cref="DbContext"/>.
    /// </summary>
    /// <typeparam name="TContext">The type of the <see cref="DbContext"/> to use.</typeparam>
    /// <param name="builder">The <see cref="ISeederBuilder"/> to configure.</param>
    /// <returns>An <see cref="ISeederBuilder"/> for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
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
    /// Registers all seeders from the specified assemblies.
    /// </summary>
    /// <param name="builder">The <see cref="ISeederBuilder"/> to configure.</param>
    /// <param name="assemblies">The assemblies to scan for seeders. If none are provided, the calling assembly is scanned.</param>
    /// <returns>An <see cref="ISeederBuilder"/> for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static ISeederBuilder RegisterEntitySeedersFromAssemblies(this ISeederBuilder builder, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (assemblies.Length == 0) assemblies = [Assembly.GetCallingAssembly()];
        builder.Services.Scan(selector => selector.FromAssemblies(assemblies)
            .AddClasses(filter => filter.AssignableTo<IBaseEntityDataSeeder>())
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        return builder;
    }

    /// <summary>
    /// Configures the seeder to load data from embedded resources in the specified assemblies.
    /// </summary>
    /// <param name="builder">The <see cref="ISeederBuilder"/> to configure.</param>
    /// <param name="assemblies">The assemblies to load embedded data from. If none are provided, the calling assembly is used.</param>
    /// <returns>An <see cref="ISeederBuilder"/> for further configuration.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    public static ISeederBuilder RegisterEmbeddedSeedDataFromAssemblies(this ISeederBuilder builder, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSingleton<ISeedDataProvider>(_ => new EmbeddedResourceSeedDataProvider(assemblies));
        return builder;
    }
}