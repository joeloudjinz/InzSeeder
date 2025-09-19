using Microsoft.Extensions.DependencyInjection;

namespace InzSeeder.Core.Builder;

/// <summary>
/// Builder implementation for configuring InzSeeder services.
/// </summary>
public class SeederBuilder : ISeederBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SeederBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public SeederBuilder(IServiceCollection services)
    {
        Services = services;
    }

    /// <inheritdoc/>
    public IServiceCollection Services { get; }
}