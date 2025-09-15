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
    /// A seeder is considered production safe if it has the <see cref="EnvironmentCompatibilityAttribute"/> 
    /// with the ProductionSafe property set to true. If no attribute is specified, the seeder 
    /// is considered not safe for production.
    /// </summary>
    /// <param name="seeder">The seeder to check for production safety.</param>
    /// <returns>True if the seeder is explicitly marked as production safe, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when seeder is null.</exception>
    public static bool IsProductionSafe(this IEntitySeeder seeder)
    {
        ArgumentNullException.ThrowIfNull(seeder);
        
        var attribute = seeder.GetType().GetCustomAttributes(typeof(EnvironmentCompatibilityAttribute), false).FirstOrDefault() as EnvironmentCompatibilityAttribute;
        
        return attribute?.ProductionSafe ?? false;
    }

    /// <summary>
    /// Checks if a seeder is allowed to run in the specified environment.
    /// A seeder is allowed to run in an environment if:
    /// 1. It doesn't have an <see cref="EnvironmentCompatibilityAttribute"/> (allowed in all environments)
    /// 2. It has the attribute but with no specific allowed environments (allowed in all environments)
    /// 3. It has the attribute with the specified environment in its AllowedEnvironments list
    /// The environment comparison is case-insensitive.
    /// </summary>
    /// <param name="seeder">The seeder to check.</param>
    /// <param name="environment">The environment name to check against.</param>
    /// <returns>True if the seeder is allowed to run in the specified environment, false otherwise.</returns>
    /// <exception cref="ArgumentNullException">Thrown when seeder or environment is null.</exception>
    public static bool IsAllowedInEnvironment(this IEntitySeeder seeder, string environment)
    {
        ArgumentNullException.ThrowIfNull(seeder);
        ArgumentNullException.ThrowIfNull(environment);
        
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
    /// Returns the <see cref="EnvironmentCompatibilityAttribute"/> associated with the seeder,
    /// which contains information about whether the seeder is production safe and which 
    /// environments it is allowed to run in. Returns null if no attribute is specified.
    /// </summary>
    /// <param name="seeder">The seeder to get environment compatibility information for.</param>
    /// <returns>The environment compatibility attribute, or null if not specified.</returns>
    /// <exception cref="ArgumentNullException">Thrown when seeder is null.</exception>
    public static EnvironmentCompatibilityAttribute? GetEnvironmentCompatibility(this IEntitySeeder seeder)
    {
        ArgumentNullException.ThrowIfNull(seeder);
        
        return seeder.GetType().GetCustomAttributes(typeof(EnvironmentCompatibilityAttribute), false).FirstOrDefault() as EnvironmentCompatibilityAttribute;
    }
}