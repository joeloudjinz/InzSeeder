using InzSeeder.Core.Models;

namespace InzSeeder.Core.Utilities;

/// <summary>
/// Utility class for environment detection and validation in the seeder application.
/// </summary>
public static class EnvironmentUtility
{
    private static string? _environment;

    /// <summary>
    /// Valid environment names for the seeder application.
    /// </summary>
    public static readonly string[] ValidEnvironments = ["Development", "Staging", "Production"];

    /// <summary>
    /// Gets the current environment for the seeder application.
    /// </summary>
    /// <returns>The current environment name.</returns>
    /// <exception cref="NullReferenceException">Thrown when environment has not been determined yet.</exception>
    public static string Environment() => _environment ?? throw new NullReferenceException("Environment is not specified");

    /// <summary>
    /// Determines the environment for the seeder application based on command-line arguments,
    /// environment variables, or throws an exception if none is found.
    /// </summary>
    /// <param name="environmentFromCommandLine">Environment specified via command-line arguments.</param>
    /// <returns>The determined environment name.</returns>
    /// <exception cref="NullReferenceException">Thrown when no environment can be determined.</exception>
    public static string DetermineEnvironment(string? environmentFromCommandLine)
    {
        // 1. Command-line argument has highest precedence.
        if (!string.IsNullOrEmpty(environmentFromCommandLine))
        {
            Console.WriteLine($"[INFO] Using environment from command line: {environmentFromCommandLine}");
            _environment = environmentFromCommandLine;
            return environmentFromCommandLine;
        }

        // 2. Fallback to custom SEEDING_ENVIRONMENT variable.
        var seedingEnvVar = System.Environment.GetEnvironmentVariable("SEEDING_ENVIRONMENT");
        if (!string.IsNullOrEmpty(seedingEnvVar))
        {
            Console.WriteLine($"[INFO] Using environment from SEEDING_ENVIRONMENT variable: {seedingEnvVar}");
            _environment = seedingEnvVar;
            return seedingEnvVar;
        }

        // 3. Fallback to standard DOTNET_ENVIRONMENT variable.
        var dotnetEnvVar = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        if (!string.IsNullOrEmpty(dotnetEnvVar))
        {
            Console.WriteLine($"[INFO] Using environment from DOTNET_ENVIRONMENT variable: {dotnetEnvVar}");
            _environment = dotnetEnvVar;
            return dotnetEnvVar;
        }

        throw new NullReferenceException("Environment is not specified");
    }

    /// <summary>
    /// Validates the seeding configuration.
    /// </summary>
    /// <param name="settings">The seeding settings to validate.</param>
    /// <returns>True if the configuration is valid, false otherwise.</returns>
    public static bool ValidateConfiguration(SeedingSettings? settings)
    {
        if (settings == null)
        {
            Console.Error.WriteLine("Seeding settings are null");
            return false;
        }

        // Validate environment names (basic validation)
        foreach (var profile in settings.Profiles.Where(profile => !ValidEnvironments.Contains(profile.Key, StringComparer.OrdinalIgnoreCase)))
        {
            Console.Error.WriteLine("Non-standard environment name detected: {0}", profile.Key);
            return false;
        }

        // Additional validation can be added here
        return true;
    }

    /// <summary>
    /// For testing purposes only. Resets the environment to allow re-setting.
    /// </summary>
    /// <param name="environment">The environment to set.</param>
    public static void ResetEnvironmentForTesting(string? environment)
    {
        _environment = environment;
    }
}