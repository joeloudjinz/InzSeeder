using InzSeeder.Core.Contracts;
using InzSeeder.Core.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace InzSeeder.Core.Extensions;

/// <summary>
/// Extension methods for IServiceProvider to work with InzSeeder.
/// </summary>
public static class ServiceProviderExtensions
{
    /// <summary>
    /// Seeds all registered data using the InzSeeder orchestrator.
    /// This method retrieves the ISeedingOrchestrator from the service provider and executes the seeding process.
    /// </summary>
    /// <param name="serviceProvider">The service provider to retrieve the seeder from.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when serviceProvider is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when ISeedingOrchestrator is not registered in the service provider.</exception>
    public static async Task RunInzSeeder(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        EnvironmentUtility.DetermineEnvironment();
        
        var seeder = serviceProvider.GetService<ISeedingOrchestrator>() 
            ?? throw new InvalidOperationException("ISeedingOrchestrator is not registered. Please ensure AddInzSeeder() is called during service registration.");
        
        await seeder.SeedDataAsync(cancellationToken);
    }
}