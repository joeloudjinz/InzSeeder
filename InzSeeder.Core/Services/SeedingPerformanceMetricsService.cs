using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Services;

/// <summary>
/// Service for collecting performance metrics during seeding operations.
/// </summary>
public class SeedingPerformanceMetricsService
{
    private readonly ILogger<SeedingPerformanceMetricsService> _logger;
    private readonly Dictionary<string, SeedingMetrics> _metrics = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedingPerformanceMetricsService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public SeedingPerformanceMetricsService(ILogger<SeedingPerformanceMetricsService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Starts measuring performance for a seeder.
    /// </summary>
    /// <param name="seederName">The name of the seeder.</param>
    /// <returns>A disposable token that stops measurement when disposed.</returns>
    public SeedingMetricsToken StartMeasurement(string seederName)
    {
        var metrics = new SeedingMetrics
        {
            SeederName = seederName,
            StartTime = DateTime.UtcNow
        };

        _metrics[seederName] = metrics;

        return new SeedingMetricsToken(this, seederName);
    }

    /// <summary>
    /// Stops measuring performance for a seeder.
    /// </summary>
    /// <param name="seederName">The name of the seeder.</param>
    /// <param name="itemCount">The number of items processed.</param>
    /// <param name="memoryUsageBytes">The memory usage in bytes.</param>
    public void StopMeasurement(string seederName, int itemCount = 0, long memoryUsageBytes = 0)
    {
        if (_metrics.TryGetValue(seederName, out var metrics))
        {
            metrics.EndTime = DateTime.UtcNow;
            metrics.Duration = metrics.EndTime - metrics.StartTime;
            metrics.ItemCount = itemCount;
            metrics.MemoryUsageBytes = memoryUsageBytes;

            _logger.LogInformation(
                "Seeder '{SeederName}' performance metrics: Duration={DurationMs}ms, Items={ItemCount}, Memory={MemoryUsageBytes} bytes",
                seederName,
                metrics.Duration.TotalMilliseconds,
                itemCount,
                memoryUsageBytes);

            // Remove the metrics from the dictionary as we've logged them
            _metrics.Remove(seederName);
        }
    }

    /// <summary>
    /// Gets all collected metrics.
    /// </summary>
    /// <returns>A collection of seeding metrics.</returns>
    public IEnumerable<SeedingMetrics> GetMetrics()
    {
        return _metrics.Values.ToList();
    }
}

/// <summary>
/// Represents performance metrics for a seeding operation.
/// </summary>
public class SeedingMetrics
{
    /// <summary>
    /// Gets or sets the name of the seeder.
    /// </summary>
    public string SeederName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the start time of the operation.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the end time of the operation.
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// Gets or sets the duration of the operation.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the number of items processed.
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// Gets or sets the memory usage in bytes.
    /// </summary>
    public long MemoryUsageBytes { get; set; }
}

/// <summary>
/// A disposable token that stops measurement when disposed.
/// </summary>
public class SeedingMetricsToken : IDisposable
{
    private readonly SeedingPerformanceMetricsService _service;
    private readonly string _seederName;
    private readonly long _startMemory;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedingMetricsToken"/> class.
    /// </summary>
    /// <param name="service">The performance metrics service.</param>
    /// <param name="seederName">The name of the seeder.</param>
    public SeedingMetricsToken(SeedingPerformanceMetricsService service, string seederName)
    {
        _service = service;
        _seederName = seederName;
        _startMemory = GC.GetTotalMemory(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            var endMemory = GC.GetTotalMemory(false);
            var memoryUsage = Math.Max(0, endMemory - _startMemory);
            _service.StopMeasurement(_seederName, memoryUsageBytes: memoryUsage);
            _disposed = true;
        }
    }
}