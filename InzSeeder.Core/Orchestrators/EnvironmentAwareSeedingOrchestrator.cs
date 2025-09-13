using InzSeeder.Core.Contracts;
using InzSeeder.Core.Extensions;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Orchestrators;

/// <summary>
/// Environment-aware orchestrator that filters seeders based on environment and configuration.
/// </summary>
public class EnvironmentAwareSeedingOrchestrator : ISeedingOrchestrator
{
    private readonly IEnumerable<IEntitySeeder> _allSeeders;
    private readonly ISeederDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly SeedingProfileValidationService _validationService;
    private readonly SeedingAuditService _auditService;
    private readonly SeedingPerformanceMetricsService _performanceMetricsService;
    private readonly SeedingMonitoringService _monitoringService;
    private readonly ILogger<EnvironmentAwareSeedingOrchestrator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentAwareSeedingOrchestrator"/> class.
    /// </summary>
    /// <param name="allSeeders">All available seeders.</param>
    /// <param name="dbContext">The database context.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="validationService">The seeding profile validation service.</param>
    /// <param name="auditService">The seeding audit service.</param>
    /// <param name="performanceMetricsService">The performance metrics service.</param>
    /// <param name="monitoringService">The monitoring service.</param>
    /// <param name="logger">The logger.</param>
    public EnvironmentAwareSeedingOrchestrator(
        IEnumerable<IEntitySeeder> allSeeders,
        ISeederDbContext dbContext,
        IConfiguration configuration,
        SeedingProfileValidationService validationService,
        SeedingAuditService auditService,
        SeedingPerformanceMetricsService performanceMetricsService,
        SeedingMonitoringService monitoringService,
        ILogger<EnvironmentAwareSeedingOrchestrator> logger
    )
    {
        _allSeeders = allSeeders;
        _dbContext = dbContext;
        _configuration = configuration;
        _validationService = validationService;
        _auditService = auditService;
        _performanceMetricsService = performanceMetricsService;
        _monitoringService = monitoringService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting environment-aware data seeding process");

        // Get seeding settings from configuration
        var settings = _configuration.GetSection(SeedingSettings.SectionName).Get<SeedingSettings>() ?? new SeedingSettings();

        // Validate settings
        if (!_validationService.ValidateSettings(settings))
        {
            _logger.LogError("Invalid seeding configuration detected. Seeding process aborted.");
            await _auditService.LogOperationAsync(
                seederName: "Orchestrator",
                operation: "ConfigurationValidation",
                details: "Invalid seeding configuration detected",
                success: false,
                errorMessage: "Invalid seeding configuration"
            );
            throw new InvalidOperationException("Invalid seeding configuration detected.");
        }

        // Get profile for the current environment
        var profile = GetProfileForEnvironment(settings);
        // TODO log the profile details using _logger.LogInformation()

        // Log the start of the seeding process
        await _auditService.LogOperationAsync(
            seederName: "Orchestrator",
            operation: "Start",
            details: $"Starting seeding process with {profile.EnabledSeeders?.Count ?? 0} seeders enabled",
            true
        );

        // Filter seeders based on profile and environment
        var seedersToRun = FilterSeedersByProfile(_allSeeders, profile).ToList();
        _logger.LogInformation("Found {SeederCount} seeders to execute after filtering", seedersToRun.Count);

        // Log which seeders will be skipped
        var allSeederNames = _allSeeders.Select(s => s.SeedName).ToHashSet();
        var seedersToRunNames = seedersToRun.Select(s => s.SeedName).ToHashSet();
        var skippedSeeders = allSeederNames.Except(seedersToRunNames);

        foreach (var skippedSeeder in skippedSeeders)
        {
            _logger.LogInformation("Seeder '{SeederName}' will be skipped based on environment configuration", skippedSeeder);
            await _auditService.LogOperationAsync(skippedSeeder, operation: "Skip", details: "Seeder skipped based on environment configuration", success: true);
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
                await _auditService.LogOperationAsync(seeder.SeedName, operation: "Execute", details: "Starting seeder execution", success: true);

                // Start performance measurement for the seeder
                var startTime = DateTime.UtcNow;
                Exception? seederException = null;
                try
                {
                    await seeder.ExecuteAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    seederException = ex;
                    throw;
                }
                finally
                {
                    // Record performance metrics and report to monitoring
                    var endTime = DateTime.UtcNow;
                    var duration = endTime - startTime;

                    // Get item count from performance metrics if available
                    var metrics = _performanceMetricsService.GetMetrics().FirstOrDefault(m => m.SeederName == seeder.SeedName);
                    var itemCount = metrics?.ItemCount ?? 0;

                    // Report to monitoring service
                    _monitoringService.ReportSeedingOperation(seeder.SeedName, duration, itemCount, seederException == null, seederException?.Message);
                }

                await _auditService.LogOperationAsync(seeder.SeedName, operation: "Complete", details: "Seeder execution completed successfully", success: true);
            }

            // Commit the transaction
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Environment-aware data seeding process completed successfully");
            await _auditService.LogOperationAsync(seederName: "Orchestrator", operation: "Complete", details: "Seeding process completed successfully", success: true);
        }
        catch (Exception ex)
        {
            // Rollback the transaction on any exception
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Environment-aware data seeding process failed");
            await _auditService.LogOperationAsync(seederName: "Orchestrator", operation: "Failed", details: "Seeding process failed", success: false, ex.Message);

            // Report failure to monitoring
            _monitoringService.ReportAlert(seederName: "Orchestrator", alertMessage: "Seeding process failed", AlertSeverity.Error);

            throw;
        }
    }

    /// <summary>
    /// Gets the seeding profile for the specified environment.
    /// </summary>
    /// <param name="settings">The seeding settings.</param>
    /// <returns>The seeding profile, or null if not found.</returns>
    private SeedingProfile GetProfileForEnvironment(SeedingSettings settings)
    {
        settings.Profiles.TryGetValue(EnvironmentUtility.Environment(), out var profile);
        return profile ?? throw new Exception($@"Environment {EnvironmentUtility.Environment()} profile is not defined the corresponding settings file.");
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