using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Services;

/// <summary>
/// Service for validating seeding profiles and configurations.
/// </summary>
public class SeedingProfileValidationService
{
    private readonly IEnumerable<IEntitySeeder> _seeders;
    private readonly ILogger<SeedingProfileValidationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedingProfileValidationService"/> class.
    /// </summary>
    /// <param name="seeders">The available seeders.</param>
    /// <param name="logger">The logger.</param>
    public SeedingProfileValidationService(IEnumerable<IEntitySeeder> seeders, ILogger<SeedingProfileValidationService> logger)
    {
        _seeders = seeders;
        _logger = logger;
    }

    /// <summary>
    /// Validates the seeding settings.
    /// </summary>
    /// <param name="settings">The seeding settings to validate.</param>
    /// <returns>True if the settings are valid, false otherwise.</returns>
    public bool ValidateSettings(SeedingSettings? settings)
    {
        if (settings == null)
        {
            _logger.LogError("Seeding settings are null");
            return false;
        }

        // Get all seeder names
        var seederNames = _seeders.Select(s => s.SeedName).ToHashSet();

        // Validate each profile
        foreach (var (environment, profile) in settings.Profiles)
        {
            if (!ValidateProfile(environment, profile, seederNames)) return false;
        }

        return true;
    }

    /// <summary>
    /// Validates a seeding profile.
    /// </summary>
    /// <param name="environment">The environment name.</param>
    /// <param name="profile">The profile to validate.</param>
    /// <param name="seederNames">The names of all available seeders.</param>
    /// <returns>True if the profile is valid, false otherwise.</returns>
    private bool ValidateProfile(string environment, SeedingProfile profile, HashSet<string> seederNames)
    {
        // Validate enabled seeders
        if (profile.EnabledSeeders != null)
        {
            foreach (var seederName in profile.EnabledSeeders.Where(seederName => !seederNames.Contains(seederName)))
            {
                _logger.LogError("Seeder '{SeederName}' referenced in profile for environment '{Environment}' does not exist", seederName, environment);
                return false;
            }
        }

        // Validate environment name
        var validEnvironments = new[] { "Development", "Staging", "Production" };
        if (!validEnvironments.Contains(environment, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Non-standard environment name detected: {Environment}", environment);
        }

        return true;
    }
}