using Microsoft.Extensions.DependencyInjection;

namespace InzSeeder.Core.Extensions;

/// <summary>
/// Builder interface for configuring InzSeeder services.
/// </summary>
public interface ISeederBuilder
{
    /// <summary>
    /// Gets the service collection.
    /// </summary>
    IServiceCollection Services { get; }
}