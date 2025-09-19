namespace InzSeeder.Core.Utilities;

/// <summary>
/// Utility class for environment detection and validation in the seeder application.
/// </summary>
internal static class EnvironmentUtility
{
    private static string? _environment;

    /// <summary>
    /// Valid environment names for the seeder application.
    /// </summary>
    public static readonly string[] ValidEnvironments = ["Development", "Staging", "Production", "Test", "IntegrationTest"];

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
    /// <param name="environment">Specified environment.</param>
    /// <returns>The determined environment name.</returns>
    /// <exception cref="NullReferenceException">Thrown when no environment can be determined.</exception>
    public static string DetermineEnvironment(string? environment = null)
    {
        // 1. Command-line argument has highest precedence.
        if (!string.IsNullOrEmpty(environment))
        {
            _environment = environment;
            return environment;
        }

        // 2. Fallback to custom SEEDING_ENVIRONMENT variable.
        var seedingEnvVar = System.Environment.GetEnvironmentVariable("SEEDING_ENVIRONMENT");
        if (!string.IsNullOrEmpty(seedingEnvVar))
        {
            _environment = seedingEnvVar;
            return seedingEnvVar;
        }

        // 3. Fallback to standard ASPNETCORE_ENVIRONMENT variable.
        var dotnetEnvVar = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (!string.IsNullOrEmpty(dotnetEnvVar))
        {
            _environment = dotnetEnvVar;
            return dotnetEnvVar;
        }
        
        throw new NullReferenceException("Environment is not specified");
    }
    
    /// <summary>
    /// Resets the environment for testing purposes.
    /// </summary>
    public static void ResetForTesting()
    {
        _environment = null;
    }
}