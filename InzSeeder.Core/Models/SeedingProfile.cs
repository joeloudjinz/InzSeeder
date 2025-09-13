namespace InzSeeder.Core.Models;

/// <summary>
/// Represents a seeding profile for a specific environment.
/// </summary>
public class SeedingProfile
{
    /// <summary>
    /// Gets or sets the list of seeders that are enabled for this profile.
    /// </summary>
    public List<string>? EnabledSeeders { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether strict mode is enabled.
    /// In strict mode, only explicitly enabled seeders will run.
    /// </summary>
    public bool StrictMode { get; set; }
}