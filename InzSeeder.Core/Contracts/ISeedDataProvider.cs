namespace InzSeeder.Core.Contracts;

/// <summary>
/// Represents a provider that reads raw seed data content and calculates its hash.
/// </summary>
internal interface ISeedDataProvider
{
    /// <summary>
    /// Gets the seed data content and its hash for the specified seed name.
    /// </summary>
    /// <param name="seedName">The name of the seed data to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A tuple containing the content and its hash, or null values if not found.</returns>
    Task<(string? content, string? hash)> GetSeedDataAsync(string seedName, CancellationToken cancellationToken);
}