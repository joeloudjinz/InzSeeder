namespace InzSeeder.Core.Models;

/// <summary>
/// Represents an audit log entry for a seeding operation.
/// </summary>
public class SeedingAuditLog
{
    /// <summary>
    /// Gets or sets the unique identifier for the audit log entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the operation occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the environment in which the operation occurred.
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the seeder that was executed.
    /// </summary>
    public string SeederName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operation that was performed (e.g., "Executed", "Skipped").
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets additional details about the operation.
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the operator who initiated the operation (if available).
    /// </summary>
    public string? Operator { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets any error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}