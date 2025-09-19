using System.Reflection;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Extensions;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Core.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Algorithms;

/// <summary>
/// Orchestrates the environment-aware data seeding process.
/// </summary>
/// <remarks>
/// This internal static class is the main entry point for executing the data seeding process.
/// It is responsible for:
/// - Validating the seeding configuration.
/// - Filtering seeders based on the current environment and seeding profile.
/// - Sorting seeders based on their dependencies.
/// - Executing the seeders within a database transaction.
/// - Rolling back the transaction if any seeder fails.
/// </remarks>
internal static class EnvironmentSeedingOrchestrator
{
    /// <summary>
    /// Orchestrates the entire data seeding process from start to finish.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="InvalidOperationException">Thrown when the seeding configuration is invalid or a critical error occurs.</exception>
    public static async Task Orchestrate(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("EnvironmentSeedingOrchestrator");
        var dbContext = serviceProvider.GetRequiredService<ISeederDbContext>();
        var settings = serviceProvider.GetRequiredService<SeederConfiguration>();
        var validationService = serviceProvider.GetRequiredService<SeedingProfileValidationService>();

        logger.LogInformation("Starting environment-aware data seeding process");

        if (!validationService.ValidateSettings(settings))
        {
            throw new InvalidOperationException("Invalid seeding configuration detected. Seeding process aborted.");
        }

        // This is a correct way to get all registered seeders.
        var allSeedersAsBaseEntitySeeder = serviceProvider.GetServices<IBaseEntityDataSeeder>().ToList();

        var profile = settings.Profile;
        logger.LogInformation("Starting seeding process with {EnabledSeedersCount} seeders enabled", profile.EnabledSeeders?.Count ?? 0);

        var seedersToRun = FilterSeedersByProfile(allSeedersAsBaseEntitySeeder, profile).ToList();
        logger.LogInformation("Found {SeederCount} seeders to execute after filtering", seedersToRun.Count);

        var allSeederNames = allSeedersAsBaseEntitySeeder.Select(s => s.SeedName).ToHashSet();
        var seedersToRunNames = seedersToRun.Select(s => s.SeedName).ToHashSet();
        var skippedSeeders = allSeederNames.Except(seedersToRunNames);

        foreach (var skippedSeeder in skippedSeeders)
        {
            logger.LogWarning("Seeder '{SeederName}' will be skipped based on configuration", skippedSeeder);
        }

        var sortedSeeders = SeederSorter.Sort(seedersToRun).ToList();
        logger.LogInformation("Executing {SeederCount} seeders in dependency order", sortedSeeders.Count);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Get the generic Execute method definition from the SeederExecutor class.
            // We need BindingFlags because it's an internal static method.
            var genericExecuteMethod = typeof(SeederExecutor).GetMethod(
                "Execute",
                BindingFlags.NonPublic | BindingFlags.Static
            );

            if (genericExecuteMethod == null)
            {
                // This would be a critical issue, indicating a problem with the seeder framework itself.
                throw new InvalidOperationException("Could not find the 'Execute' method in SeederExecutor.");
            }

            foreach (var seeder in sortedSeeders)
            {
                logger.LogInformation("Executing seeder '{SeederName}'", seeder.SeedName);

                try
                {
                    // Find the specific IEntityDataSeeder<,> interface implemented by this instance.
                    var seederInterface = seeder.GetType().GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityDataSeeder<,>));
                    if (seederInterface == null)
                    {
                        logger.LogError("Could not find the IEntityDataSeeder<,> interface on seeder '{SeederName}'. Skipping.", seeder.SeedName);
                        continue;
                    }

                    // Extract the TEntity and TModel types from the interface.
                    var genericArguments = seederInterface.GetGenericArguments();
                    var entityType = genericArguments[0];
                    var modelType = genericArguments[1];
                    
                    // Create a concrete method by supplying the generic types.
                    var concreteExecuteMethod = genericExecuteMethod.MakeGenericMethod(entityType, modelType);

                    // Invoke the method. The first parameter is null because it's a static method.
                    var task = (Task?)concreteExecuteMethod.Invoke(null, [seeder, serviceProvider, cancellationToken]);
                    if (task is null) throw new InvalidOperationException("Could not create async task from 'Execute' method.");

                    await task;
                }
                catch (Exception ex)
                {
                    // Catching exceptions per-seeder allows the orchestrator to continue if one fails,
                    // though the final transaction rollback means no partial data will be committed.
                    logger.LogCritical(ex, "Seeder '{SeederName}' execution failed catastrophically.", seeder.SeedName);
                    throw; // Re-throw to ensure the transaction is rolled back.
                }

                logger.LogInformation("Seeder '{SeederName}' execution completed", seeder.SeedName);
            }

            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("Environment-aware data seeding process completed successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            logger.LogError(ex, "Environment-aware data seeding process failed. Transaction has been rolled back.");
            // Re-throw so the application startup to fail.
            throw;
        }
    }

    /// <summary>
    /// Filters a list of seeders based on the provided seeding profile and current environment.
    /// </summary>
    /// <param name="seeders">The collection of all registered seeders.</param>
    /// <param name="profile">The seeding profile containing filtering rules.</param>
    /// <returns>An enumerable of seeders that should be executed.</returns>
    private static IEnumerable<IBaseEntityDataSeeder> FilterSeedersByProfile(IEnumerable<IBaseEntityDataSeeder> seeders, SeedingProfile profile)
    {
        return seeders.Where(seeder =>
        {
            // Check if seeder is explicitly enabled for this profile
            if (profile.EnabledSeeders != null && profile.EnabledSeeders.Contains(seeder.SeedName)) return true;

            // If in strict mode, only explicitly enabled seeders should run
            if (profile.StrictMode && (profile.EnabledSeeders == null || !profile.EnabledSeeders.Contains(seeder.SeedName))) return false;

            // Check if seeder has environment awareness
            if (seeder is IEnvironmentAwareSeeder envAwareSeeder) return envAwareSeeder.ShouldRunInEnvironment(EnvironmentUtility.Environment());

            // Check if seeder has environment compatibility attribute
            if (!seeder.IsAllowedInEnvironment(EnvironmentUtility.Environment())) return false;

            // Default behavior - run if not explicitly disabled
            return profile.EnabledSeeders == null || profile.EnabledSeeders.Contains(seeder.SeedName);
        });
    }
}