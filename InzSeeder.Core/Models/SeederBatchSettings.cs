namespace InzSeeder.Core.Models;

/// <summary>
/// Configuration settings for batch processing in seeders.
/// </summary>
public class SeederBatchSettings
{
    /// <summary>
    /// Gets or sets the default batch size for seeders.
    /// </summary>
    public int DefaultBatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the batch size for specific seeders.
    /// </summary>
    public Dictionary<string, int> SeederBatchSizes { get; set; } = new();
}