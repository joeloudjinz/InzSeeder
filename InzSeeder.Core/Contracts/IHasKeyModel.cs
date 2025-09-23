namespace InzSeeder.Core.Contracts;

/// <summary>
/// Interface for seed models and entities that have a string key for reference resolution.
/// </summary>
public interface IHasKeyModel
{
    /// <summary>
    /// Gets the string key for this model.
    /// </summary>
    string Key { get; }
}