namespace InzSeeder.Core.Attributes;

/// <summary>
/// Attribute to mark a seeder with environment compatibility information.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class EnvironmentCompatibilityAttribute : Attribute
{
    /// <summary>
    /// Gets a value indicating whether this seeder is safe to run in production.
    /// </summary>
    public bool ProductionSafe { get; }

    /// <summary>
    /// Gets the list of environments in which this seeder is allowed to run.
    /// </summary>
    public string[] AllowedEnvironments { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentCompatibilityAttribute"/> class.
    /// </summary>
    /// <param name="productionSafe">Whether this seeder is safe to run in production.</param>
    /// <param name="allowedEnvironments">The environments in which this seeder is allowed to run.</param>
    public EnvironmentCompatibilityAttribute(bool productionSafe = false, params string[] allowedEnvironments)
    {
        ProductionSafe = productionSafe;
        AllowedEnvironments = allowedEnvironments ?? [];
    }
}