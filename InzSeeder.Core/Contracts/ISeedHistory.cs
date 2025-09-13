namespace InzSeeder.Core.Contracts;

/// <summary>
/// Represents the history of a seed operation.
/// </summary>
public interface ISeedHistory
{
    /// <summary>
    /// Gets or sets the unique identifier for this seed history record.
    /// </summary>
    int Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier for the seed operation.
    /// </summary>
    string SeedIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the hash of the seed content.
    /// </summary>
    string ContentHash { get; set; }

    /// <summary>
    /// Gets or sets the UTC date when the seed was applied.
    /// </summary>
    DateTime AppliedDateUtc { get; set; }
}