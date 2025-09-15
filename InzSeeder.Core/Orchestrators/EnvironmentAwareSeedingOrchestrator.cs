using InzSeeder.Core.Contracts;
using InzSeeder.Core.Extensions;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Core.Utilities;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Orchestrators;

/// <summary>
/// Environment-aware orchestrator that filters seeders based on environment and configuration.
/// </summary>
public class EnvironmentAwareSeedingOrchestrator : ISeedingOrchestrator
{
    private readonly IEnumerable<IEntitySeeder> _allSeeders;
    private readonly ISeederDbContext _dbContext;
    private readonly SeederConfiguration _seederConfiguration; // Changed from IConfiguration
    private readonly SeedingProfileValidationService _validationService;
    private readonly ILogger<EnvironmentAwareSeedingOrchestrator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentAwareSeedingOrchestrator"/> class.
    /// </summary>
    /// <param name="allSeeders">All available seeders.</param>
    /// <param name="dbContext">The database context.</param>
    /// <param name="seederConfiguration">The seeding settings.</param>
    /// <param name="validationService">The seeding profile validation service.</param>
    /// <param name="logger">The logger.</param>
    public EnvironmentAwareSeedingOrchestrator(
        IEnumerable<IEntitySeeder> allSeeders,
        ISeederDbContext dbContext,
        SeederConfiguration seederConfiguration,
        SeedingProfileValidationService validationService,
        ILogger<EnvironmentAwareSeedingOrchestrator> logger
    )
    {
        _allSeeders = allSeeders;
        _dbContext = dbContext;
        _seederConfiguration = seederConfiguration;
        _validationService = validationService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting environment-aware data seeding process");

        // Use the injected settings directly instead of retrieving from configuration
        var settings = _seederConfiguration;

        // Validate settings
        if (!_validationService.ValidateSettings(settings)) throw new InvalidOperationException("Invalid seeding configuration detected. Seeding process aborted.");

        // Get profile for the current environment
        var profile = settings.Profile;

        // Log the start of the seeding process
        _logger.LogInformation("Starting seeding process with {EnabledSeedersCount} seeders enabled", profile.EnabledSeeders?.Count ?? 0);

        // Filter seeders based on profile and environment
        var seedersToRun = FilterSeedersByProfile(_allSeeders, profile).ToList();
        _logger.LogInformation("Found {SeederCount} seeders to execute after filtering", seedersToRun.Count);

        // Log which seeders will be skipped
        var allSeederNames = _allSeeders.Select(s => s.SeedName).ToHashSet();
        var seedersToRunNames = seedersToRun.Select(s => s.SeedName).ToHashSet();
        var skippedSeeders = allSeederNames.Except(seedersToRunNames);

        foreach (var skippedSeeder in skippedSeeders)
        {
            _logger.LogWarning("Seeder '{SeederName}' will be skipped based on environment configuration", skippedSeeder);
        }

        // Sort seeders based on their dependencies
        var sortedSeeders = SeederSorter.Sort(seedersToRun).ToList();
        _logger.LogInformation("Executing {SeederCount} seeders in dependency order", sortedSeeders.Count);

        // Create a database transaction
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Execute each seeder in the sorted order
            foreach (var seeder in sortedSeeders)
            {
                _logger.LogInformation("Executing seeder '{SeederName}'", seeder.SeedName);

                try
                {
                    await seeder.ExecuteAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Seeder {SeederName} execution failed", seeder.SeedName);
                }
                finally
                {
                    _logger.LogInformation("Seeder '{SeederName}' execution completed", seeder.SeedName);
                }
            }

            // Commit the transaction
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Environment-aware data seeding process completed successfully");
        }
        catch (Exception ex)
        {
            // Rollback the transaction on any exception
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Environment-aware data seeding process failed");
        }
    }

    /// <summary>
    /// Filters seeders based on the specified profile and environment.
    /// </summary>
    /// <param name="seeders">The seeders to filter.</param>
    /// <param name="profile">The seeding profile.</param>
    /// <returns>The filtered seeders.</returns>
    private IEnumerable<IEntitySeeder> FilterSeedersByProfile(IEnumerable<IEntitySeeder> seeders, SeedingProfile profile)
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