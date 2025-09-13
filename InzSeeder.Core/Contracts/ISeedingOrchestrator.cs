namespace InzSeeder.Core.Contracts;

/// <summary>
/// Represents an orchestrator that manages the overall seeding process flow.
/// </summary>
public interface ISeedingOrchestrator
{
    /// <summary>
    /// Seeds all registered data.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SeedDataAsync(CancellationToken cancellationToken);
}