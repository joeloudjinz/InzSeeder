using InzSeeder.Core.Adapters;
using InzSeeder.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InzSeeder.Core.Extensions;

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