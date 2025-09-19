using InzSeeder.Core.Attributes;
using InzSeeder.Core.Contracts;

namespace InzSeeder.Core.Extensions;

/// <summary>
/// Extension methods for working with environment-aware seeders.
/// </summary>
public static class EnvironmentAwareSeederExtensions
{
    public static bool IsProductionSafe(this IBaseEntityDataSeeder seeder)
    {
        ArgumentNullException.ThrowIfNull(seeder);

        var attribute = seeder.GetType().GetCustomAttributes(typeof(EnvironmentCompatibilityAttribute), false).FirstOrDefault() as EnvironmentCompatibilityAttribute;

        return attribute?.ProductionSafe ?? false;
    }

    public static bool IsAllowedInEnvironment(this IBaseEntityDataSeeder seeder, string environment)
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

    public static EnvironmentCompatibilityAttribute? GetEnvironmentCompatibility(this IBaseEntityDataSeeder seeder)
    {
        ArgumentNullException.ThrowIfNull(seeder);

        return seeder.GetType().GetCustomAttributes(typeof(EnvironmentCompatibilityAttribute), false).FirstOrDefault() as EnvironmentCompatibilityAttribute;
    }
}