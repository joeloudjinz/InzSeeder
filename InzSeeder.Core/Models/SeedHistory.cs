using InzSeeder.Core.Contracts;

namespace InzSeeder.Core.Models;

/// <summary>
/// Represents the history of a seed operation.
/// </summary>
public class SeedHistory : ISeedHistory
{
    /// <summary>
    /// Gets or sets the unique identifier for this seed history record.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the seed operation.
    /// </summary>
    public string SeedIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the hash of the seed content.
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UTC date when the seed was applied.
    /// </summary>
    public DateTime AppliedDateUtc { get; set; }
}