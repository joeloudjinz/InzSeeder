namespace InzSeeder.Core.Contracts;

/// <summary>
/// Represents an entity that can be marked as system-owned.
/// </summary>
public interface ISystemOwnedEntity
{
    /// <summary>
    /// Gets or sets a value indicating whether this entity is owned by the system.
    /// </summary>
    bool IsSystemOwned { get; set; }
}