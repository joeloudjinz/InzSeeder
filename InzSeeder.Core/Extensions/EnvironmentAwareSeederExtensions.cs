using InzSeeder.Core.Attributes;
using InzSeeder.Core.Contracts;

namespace InzSeeder.Core.Extensions;

/// <summary>
/// Extension methods for working with environment-aware seeders.
/// </summary>
public static class EnvironmentAwareSeederExtensions
{
    /// <summary>
    /// Checks if a seeder is marked as production safe.
    /// </summary>
    /// <param name="seeder">The seeder to check.</param>
    /// <returns>True if the seeder is marked as production safe, false otherwise.</returns>
    public static bool IsProductionSafe(this IEntitySeeder seeder)
    {
        var attribute = seeder.GetType().GetCustomAttributes(typeof(EnvironmentCompatibilityAttribute), false).FirstOrDefault() as EnvironmentCompatibilityAttribute;
        
        return attribute?.ProductionSafe ?? false;
    }

    /// <summary>
    /// Checks if a seeder is allowed to run in the specified environment.
    /// </summary>
    /// <param name="seeder">The seeder to check.</param>
    /// <param name="environment">The environment to check against.</param>
    /// <returns>True if the seeder is allowed to run in the specified environment, false otherwise.</returns>
    public static bool IsAllowedInEnvironment(this IEntitySeeder seeder, string environment)
    {
        var attribute = seeder.GetType().GetCustomAttributes(typeof(EnvironmentCompatibilityAttribute), false).FirstOrDefault() as EnvironmentCompatibilityAttribute;
        
        // If no attribute is specified, allow in all environments
        if (attribute == null) return true;
        
        // If allowed environments list is empty, allow in all environments
        if (attribute.AllowedEnvironments.Length == 0) return true;
        
        // Check if the environment is in the allowed list (case-insensitive)
        return attribute.AllowedEnvironments.Contains(environment, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the environment compatibility information for a seeder.
    /// </summary>
    /// <param name="seeder">The seeder to get information for.</param>
    /// <returns>The environment compatibility attribute, or null if not specified.</returns>
    public static EnvironmentCompatibilityAttribute? GetEnvironmentCompatibility(this IEntitySeeder seeder)
    {
        return seeder.GetType().GetCustomAttributes(typeof(EnvironmentCompatibilityAttribute), false).FirstOrDefault() as EnvironmentCompatibilityAttribute;
    }
}