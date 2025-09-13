using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Services;

/// <summary>
/// Service for integrating seeding operations with application monitoring systems.
/// </summary>
public class SeedingMonitoringService
{
    private readonly ILogger<SeedingMonitoringService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedingMonitoringService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public SeedingMonitoringService(ILogger<SeedingMonitoringService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Reports a seeding operation to the monitoring system.
    /// </summary>
    /// <param name="seederName">The name of the seeder.</param>
    /// <param name="duration">The duration of the operation.</param>
    /// <param name="itemCount">The number of items processed.</param>
    /// <param name="success">Whether the operation was successful.</param>
    /// <param name="errorMessage">Error message if the operation failed.</param>
    public void ReportSeedingOperation(
        string seederName,
        TimeSpan duration,
        int itemCount,
        bool success,
        string? errorMessage = null)
    {
        try
        {
            // In a real implementation, this would integrate with a monitoring system like:
            // - Application Insights
            // - Prometheus
            // - Datadog
            // - New Relic
            // - etc.

            if (success)
            {
                _logger.LogInformation(
                    "Seeding Operation Completed: Seeder={SeederName}, Duration={DurationMs}ms, Items={ItemCount}",
                    seederName,
                    duration.TotalMilliseconds,
                    itemCount);

                // Example of what you might do with a real monitoring system:
                // telemetryClient.GetMetric("Seeding.Duration").TrackValue(duration.TotalMilliseconds);
                // telemetryClient.GetMetric("Seeding.ItemCount").TrackValue(itemCount);
                // telemetryClient.TrackEvent("Seeding.Completed", new Dictionary<string, string>
                // {
                //     ["SeederName"] = seederName,
                //     ["ItemCount"] = itemCount.ToString()
                // });
            }
            else
            {
                _logger.LogError(
                    "Seeding Operation Failed: Seeder={SeederName}, Duration={DurationMs}ms, Error={ErrorMessage}",
                    seederName,
                    duration.TotalMilliseconds,
                    errorMessage);

                // Example of what you might do with a real monitoring system:
                // telemetryClient.TrackEvent("Seeding.Failed", new Dictionary<string, string>
                // {
                //     ["SeederName"] = seederName,
                //     ["Error"] = errorMessage ?? "Unknown error"
                // });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to report seeding operation to monitoring system");
        }
    }

    /// <summary>
    /// Reports an alert for a seeding operation.
    /// </summary>
    /// <param name="seederName">The name of the seeder.</param>
    /// <param name="alertMessage">The alert message.</param>
    /// <param name="severity">The severity of the alert.</param>
    public void ReportAlert(string seederName, string alertMessage, AlertSeverity severity)
    {
        try
        {
            _logger.LogWarning(
                "Seeding Alert: Seeder={SeederName}, Severity={Severity}, Message={AlertMessage}",
                seederName,
                severity,
                alertMessage);

            // Example of what you might do with a real monitoring system:
            // telemetryClient.TrackEvent("Seeding.Alert", new Dictionary<string, string>
            // {
            //     ["SeederName"] = seederName,
            //     ["Severity"] = severity.ToString(),
            //     ["Message"] = alertMessage
            // });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to report seeding alert to monitoring system");
        }
    }
}

/// <summary>
/// Represents the severity of a seeding alert.
/// </summary>
public enum AlertSeverity
{
    /// <summary>
    /// Informational alert.
    /// </summary>
    Info,

    /// <summary>
    /// Warning alert.
    /// </summary>
    Warning,

    /// <summary>
    /// Error alert.
    /// </summary>
    Error,

    /// <summary>
    /// Critical alert.
    /// </summary>
    Critical
}