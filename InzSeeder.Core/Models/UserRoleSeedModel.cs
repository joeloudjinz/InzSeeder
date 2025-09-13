namespace InzSeeder.Core.Models;

/// <summary>
/// Model for seeding user-role relationship data.
/// </summary>
public class UserRoleSeedModel
{
    /// <summary>
    /// Gets or sets the stable key for identifying this seeded entity.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key of the user.
    /// </summary>
    public string UserKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the key of the role.
    /// </summary>
    public string RoleKey { get; set; } = string.Empty;
}