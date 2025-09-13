namespace InzSeeder.Core.Contracts;

/// <summary>
/// Represents a self-contained, executable seeder unit.
/// </summary>
public interface IEntitySeeder
{
    /// <summary>
    /// Gets the unique name of this seeder.
    /// </summary>
    string SeedName { get; }

    /// <summary>
    /// Gets the collection of seeder types that this seeder depends on.
    /// </summary>
    IEnumerable<Type> Dependencies { get; }

    /// <summary>
    /// Executes the seeding logic.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteAsync(CancellationToken cancellationToken);
}