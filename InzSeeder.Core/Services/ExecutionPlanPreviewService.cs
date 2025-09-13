using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Utilities;

namespace InzSeeder.Core.Services;

/// <summary>
/// Service for previewing the seeder execution plan.
/// </summary>
public class ExecutionPlanPreviewService
{
    private readonly IEnumerable<IEntitySeeder> _allSeeders;
    private readonly SeedingProfileValidationService _validationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionPlanPreviewService"/> class.
    /// </summary>
    /// <param name="allSeeders">All available seeders.</param>
    /// <param name="validationService">The seeding profile validation service.</param>
    public ExecutionPlanPreviewService(IEnumerable<IEntitySeeder> allSeeders, SeedingProfileValidationService validationService)
    {
        _allSeeders = allSeeders;
        _validationService = validationService;
    }

    /// <summary>
    /// Shows a preview of the execution plan.
    /// </summary>
    /// <param name="settings">The seeding settings to use for the preview.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    public async Task ShowPreviewAsync(SeederConfiguration settings, CancellationToken cancellationToken)
    {
        Console.WriteLine("Seeder Execution Plan Preview");
        Console.WriteLine("=============================");

        // Validate settings
        if (!_validationService.ValidateSettings(settings))
        {
            Console.WriteLine("Invalid seeding configuration detected.");
            return;
        }

        // Get profile for the current environment
        var profile = GetProfileForEnvironment(settings);

        Console.WriteLine($"Strict Mode: {profile.StrictMode}");

        // Show all available seeders
        Console.WriteLine("\nAvailable Seeders:");
        foreach (var seeder in _allSeeders)
        {
            Console.WriteLine($"  - {seeder.SeedName}");
        }

        // Show which seeders would run
        var seedersToRun = FilterSeedersByProfile(_allSeeders, profile).ToList();
        Console.WriteLine("\nSeeders That Would Run:");
        foreach (var seeder in seedersToRun)
        {
            Console.WriteLine($"  - {seeder.SeedName}");
        }

        // Show which seeders would be skipped
        var allSeederNames = _allSeeders.Select(s => s.SeedName).ToHashSet();
        var seedersToRunNames = seedersToRun.Select(s => s.SeedName).ToHashSet();
        var skippedSeeders = allSeederNames.Except(seedersToRunNames).ToList();

        if (skippedSeeders.Count != 0)
        {
            Console.WriteLine("\nSeeders That Would Be Skipped:");
            foreach (var skippedSeeder in skippedSeeders)
            {
                Console.WriteLine($"  - {skippedSeeder}");
            }
        }

        // Show dependency information
        Console.WriteLine("\nDependency Information:");
        ShowDependencyInformation();

        await Task.CompletedTask;
    }

    /// <summary>
    /// Gets the seeding profile for the specified environment.
    /// </summary>
    /// <param name="settings">The seeding settings.</param>
    /// <returns>The seeding profile, or null if not found.</returns>
    private SeedingProfile GetProfileForEnvironment(SeederConfiguration settings)
    {
        settings.Profiles.TryGetValue(EnvironmentUtility.Environment(), out var profile);
        return profile ?? throw new Exception($"Environment [{EnvironmentUtility.Environment()}] profile is not defined the corresponding settings file.");
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

            // Default behavior - run if not explicitly disabled
            return profile.EnabledSeeders == null || profile.EnabledSeeders.Contains(seeder.SeedName);
        });
    }

    /// <summary>
    /// Shows dependency information for seeders.
    /// </summary>
    private void ShowDependencyInformation()
    {
        foreach (var seeder in _allSeeders)
        {
            var dependencies = seeder.Dependencies.ToList();
            if (dependencies.Count == 0) continue;

            Console.WriteLine($"  {seeder.SeedName} depends on:");
            foreach (var dependency in dependencies)
            {
                Console.WriteLine($"    - {dependency.Name}");
            }
        }
    }
}