namespace InzSeeder.Core.Models;

public class SeederConfiguration
{
    /// <summary>
    /// Gets or sets the profile of the current environment.
    /// </summary>
    public SeedingProfile Profile { get; set; } = new();

    /// <summary>
    /// Gets or sets the batch processing settings.
    /// </summary>
    public SeederBatchSettings BatchSettings { get; set; } = new();
}