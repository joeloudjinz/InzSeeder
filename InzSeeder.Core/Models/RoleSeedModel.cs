namespace InzSeeder.Core.Models;

/// <summary>
/// Model for seeding role data.
/// </summary>
public class RoleSeedModel
{
    /// <summary>
    /// Gets or sets the stable key for identifying this seeded entity.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the role.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normalized name of the role.
    /// </summary>
    public string NormalizedName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the role.
    /// </summary>
    public string? Description { get; set; }
}