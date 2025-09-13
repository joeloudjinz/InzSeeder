namespace InzSeeder.Core.Models;

/// <summary>
/// Configuration settings for data provisioning.
/// </summary>
public class SeedingSettings
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Seeding";

    /// <summary>
    /// Gets or sets a value indicating whether seeding is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the environment for which seeding is being performed.
    /// </summary>
    public string? Environment { get; set; }

    /// <summary>
    /// Gets or sets the profiles for different environments.
    /// </summary>
    public Dictionary<string, SeedingProfile> Profiles { get; set; } = new();

    /// <summary>
    /// Gets or sets the batch processing settings.
    /// </summary>
    public SeederBatchSettings BatchSettings { get; set; } = new();
}