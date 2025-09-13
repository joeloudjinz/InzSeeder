namespace InzSeeder.Core.Models;

/// <summary>
/// Model for seeding system variable data.
/// </summary>
public class SystemVariableSeedModel
{
    /// <summary>
    /// Gets or sets the stable key for identifying this seeded entity.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the system variable.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the system variable.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the value type of the system variable.
    /// </summary>
    public string ValueType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the system variable.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether the system variable is server-side only.
    /// </summary>
    public bool IsServerSideOnly { get; set; }

    /// <summary>
    /// Gets or sets which platforms the system variable is shared with.
    /// </summary>
    public string SharedWith { get; set; } = string.Empty;
}