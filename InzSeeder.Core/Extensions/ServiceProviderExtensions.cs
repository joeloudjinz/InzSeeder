using InzSeeder.Core.Algorithms;
using InzSeeder.Core.Utilities;

namespace InzSeeder.Core.Extensions;

/// <summary>
/// Provides extension methods for <see cref="IServiceProvider"/> to interact with InzSeeder.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Executes the data seeding process.
    /// </summary>
    /// <param name="serviceProvider">The service provider containing the registered seeder services.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous seeding operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if required seeder services are not registered.</exception>
    public static async Task RunInzSeeder(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        EnvironmentUtility.DetermineEnvironment();
        await EnvironmentSeedingOrchestrator.Orchestrate(serviceProvider, cancellationToken);
    }
}