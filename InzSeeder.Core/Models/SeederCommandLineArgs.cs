namespace InzSeeder.Core.Models;

/// <summary>
/// Represents the command-line arguments for the seeder.
/// </summary>
public class SeederCommandLineArgs
{
    /// <summary>
    /// Gets or sets the environment to use for seeding.
    /// </summary>
    public string? Environment { get; set; }

    /// <summary>
    /// Gets or sets the seeders to force run, regardless of environment.
    /// </summary>
    public List<string>? Force { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to perform a dry run (show what would be executed without running).
    /// </summary>
    public bool DryRun { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show a preview of the execution plan.
    /// </summary>
    public bool Preview { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to override safety checks.
    /// </summary>
    public bool Unsafe { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to run health checks.
    /// </summary>
    public bool HealthCheck { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to purge all existing records from the database.
    /// </summary>
    public bool Purge { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to bypass confirmation for the purge operation.
    /// </summary>
    public bool Yes { get; set; }
}