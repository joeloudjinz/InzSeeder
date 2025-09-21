using InzSeeder.Core.StressTest;
using InzSeeder.Core.StressTest.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InzSeeder.Core.StressTest.Extensions;

/// <summary>
/// Extension methods for configuring stress testing services.
/// </summary>
public static class StressTestServiceCollectionExtensions
{
    /// <summary>
    /// Adds stress test services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddStressTestServices(this IServiceCollection services)
    {
        services.AddSingleton<StressTestDataGenerator>();
        services.AddSingleton<StressTestMetricsCollector>();
        services.AddSingleton<StressTestReporter>();
        services.AddSingleton<StressTestRunner>();
        
        return services;
    }
}