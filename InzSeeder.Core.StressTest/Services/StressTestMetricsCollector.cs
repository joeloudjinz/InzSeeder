using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.StressTest.Services;

/// <summary>
/// Enhanced metrics collector for stress testing with additional system-level metrics.
/// </summary>
public class StressTestMetricsCollector
{
    private readonly ILogger<StressTestMetricsCollector> _logger;
    private readonly Process _currentProcess;
    private readonly List<SystemMetricsSnapshot> _systemMetricsHistory;
    private readonly Dictionary<string, StressTestMetrics> _metrics;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="StressTestMetricsCollector"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public StressTestMetricsCollector(ILogger<StressTestMetricsCollector> logger)
    {
        _logger = logger;
        _currentProcess = Process.GetCurrentProcess();
        _systemMetricsHistory = [];
        _metrics = new Dictionary<string, StressTestMetrics>();
    }
    
    /// <summary>
    /// Starts measuring performance for a seeder with enhanced metrics collection.
    /// </summary>
    /// <param name="seederName">The name of the seeder.</param>
    /// <returns>A disposable token that stops measurement when disposed.</returns>
    public StressTestMetricsToken StartMeasurement(string seederName)
    {
        // Record initial system metrics
        var initialSnapshot = CaptureSystemMetrics();
        _systemMetricsHistory.Add(initialSnapshot);
        
        var metrics = new StressTestMetrics
        {
            SeederName = seederName,
            StartTime = DateTime.UtcNow,
            InitialSystemMetrics = initialSnapshot
        };
        
        _metrics[seederName] = metrics;
        
        return new StressTestMetricsToken(this, seederName, initialSnapshot);
    }
    
    /// <summary>
    /// Stops measuring performance for a seeder with enhanced metrics collection.
    /// </summary>
    /// <param name="seederName">The name of the seeder.</param>
    /// <param name="memoryUsageBytes">The memory usage in bytes.</param>
    /// <param name="itemCount">The number of items processed.</param>
    /// <param name="finalSnapshot">The final system metrics snapshot.</param>
    public void StopMeasurement(string seederName, long memoryUsageBytes, int itemCount, SystemMetricsSnapshot finalSnapshot)
    {
        // Record final system metrics
        _systemMetricsHistory.Add(finalSnapshot);
        
        if (_metrics.TryGetValue(seederName, out var metrics))
        {
            metrics.EndTime = DateTime.UtcNow;
            metrics.Duration = metrics.EndTime - metrics.StartTime;
            metrics.ItemCount = itemCount;
            metrics.MemoryUsageBytes = memoryUsageBytes;
            metrics.FinalSystemMetrics = finalSnapshot;
        }
        
        _logger.LogInformation(
            "Seeder '{SeederName}' performance metrics: Duration={DurationMs}ms, Items={ItemCount}, Memory={MemoryUsageBytes} bytes, CPU={CpuTimeMs}ms",
            seederName,
            metrics?.Duration.TotalMilliseconds ?? 0,
            itemCount,
            memoryUsageBytes,
            finalSnapshot.CpuTime.TotalMilliseconds
        );
    }
    
    /// <summary>
    /// Captures a snapshot of system metrics.
    /// </summary>
    /// <returns>A system metrics snapshot.</returns>
    private SystemMetricsSnapshot CaptureSystemMetrics()
    {
        _currentProcess.Refresh();
        
        return new SystemMetricsSnapshot
        {
            Timestamp = DateTime.UtcNow,
            WorkingSetMemory = _currentProcess.WorkingSet64,
            PrivateMemory = _currentProcess.PrivateMemorySize64,
            VirtualMemory = _currentProcess.VirtualMemorySize64,
            CpuTime = _currentProcess.TotalProcessorTime,
            Threads = _currentProcess.Threads.Count,
            Handles = _currentProcess.HandleCount
        };
    }
    
    /// <summary>
    /// Gets the collected system metrics history.
    /// </summary>
    /// <returns>A collection of system metrics snapshots.</returns>
    public IEnumerable<SystemMetricsSnapshot> GetSystemMetricsHistory()
    {
        return _systemMetricsHistory.AsReadOnly();
    }
    
    /// <summary>
    /// Gets the collected metrics.
    /// </summary>
    /// <returns>A collection of stress test metrics.</returns>
    public IEnumerable<StressTestMetrics> GetMetrics()
    {
        return _metrics.Values.ToList();
    }
    
    /// <summary>
    /// Gets the overall stress test metrics.
    /// </summary>
    /// <returns>Overall stress test metrics.</returns>
    public StressTestSummaryMetrics GetSummaryMetrics()
    {
        var snapshots = _systemMetricsHistory.OrderBy(s => s.Timestamp).ToList();
        if (snapshots.Count == 0)
        {
            return new StressTestSummaryMetrics();
        }
        
        var first = snapshots.First();
        var last = snapshots.Last();
        
        return new StressTestSummaryMetrics
        {
            Duration = last.Timestamp - first.Timestamp,
            PeakWorkingSetMemory = snapshots.Max(s => s.WorkingSetMemory),
            PeakPrivateMemory = snapshots.Max(s => s.PrivateMemory),
            PeakVirtualMemory = snapshots.Max(s => s.VirtualMemory),
            TotalCpuTime = last.CpuTime - first.CpuTime,
            AverageThreadCount = (int)snapshots.Average(s => s.Threads),
            AverageHandleCount = (int)snapshots.Average(s => s.Handles)
        };
    }
}

/// <summary>
/// Represents enhanced performance metrics for a seeding operation during stress testing.
/// </summary>
public class StressTestMetrics
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
    
    /// <summary>
    /// Gets or sets the initial system metrics snapshot.
    /// </summary>
    public SystemMetricsSnapshot? InitialSystemMetrics { get; set; }
    
    /// <summary>
    /// Gets or sets the final system metrics snapshot.
    /// </summary>
    public SystemMetricsSnapshot? FinalSystemMetrics { get; set; }
}

/// <summary>
/// Represents a snapshot of system metrics.
/// </summary>
public class SystemMetricsSnapshot
{
    /// <summary>
    /// Gets or sets the timestamp of the snapshot.
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// Gets or sets the working set memory in bytes.
    /// </summary>
    public long WorkingSetMemory { get; set; }
    
    /// <summary>
    /// Gets or sets the private memory in bytes.
    /// </summary>
    public long PrivateMemory { get; set; }
    
    /// <summary>
    /// Gets or sets the virtual memory in bytes.
    /// </summary>
    public long VirtualMemory { get; set; }
    
    /// <summary>
    /// Gets or sets the total CPU time.
    /// </summary>
    public TimeSpan CpuTime { get; set; }
    
    /// <summary>
    /// Gets or sets the number of threads.
    /// </summary>
    public int Threads { get; set; }
    
    /// <summary>
    /// Gets or sets the number of handles.
    /// </summary>
    public int Handles { get; set; }
}

/// <summary>
/// Represents summary metrics for the entire stress test.
/// </summary>
public class StressTestSummaryMetrics
{
    /// <summary>
    /// Gets or sets the total duration of the test.
    /// </summary>
    public TimeSpan Duration { get; set; }
    
    /// <summary>
    /// Gets or sets the peak working set memory usage in bytes.
    /// </summary>
    public long PeakWorkingSetMemory { get; set; }
    
    /// <summary>
    /// Gets or sets the peak private memory usage in bytes.
    /// </summary>
    public long PeakPrivateMemory { get; set; }
    
    /// <summary>
    /// Gets or sets the peak virtual memory usage in bytes.
    /// </summary>
    public long PeakVirtualMemory { get; set; }
    
    /// <summary>
    /// Gets or sets the total CPU time consumed.
    /// </summary>
    public TimeSpan TotalCpuTime { get; set; }
    
    /// <summary>
    /// Gets or sets the average thread count during the test.
    /// </summary>
    public int AverageThreadCount { get; set; }
    
    /// <summary>
    /// Gets or sets the average handle count during the test.
    /// </summary>
    public int AverageHandleCount { get; set; }
}

/// <summary>
/// A disposable token that stops measurement when disposed with enhanced metrics collection.
/// </summary>
public class StressTestMetricsToken : IDisposable
{
    private readonly StressTestMetricsCollector _service;
    private readonly string _seederName;
    private readonly SystemMetricsSnapshot _initialSnapshot;
    private readonly long _startMemory;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="StressTestMetricsToken"/> class.
    /// </summary>
    /// <param name="service">The performance metrics service.</param>
    /// <param name="seederName">The name of the seeder.</param>
    /// <param name="initialSnapshot">The initial system metrics snapshot.</param>
    public StressTestMetricsToken(StressTestMetricsCollector service, string seederName, SystemMetricsSnapshot initialSnapshot)
    {
        _service = service;
        _seederName = seederName;
        _initialSnapshot = initialSnapshot;
        _startMemory = GC.GetTotalMemory(false);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;

        var endMemory = GC.GetTotalMemory(false);
        var memoryUsage = Math.Max(0, endMemory - _startMemory);
        var finalSnapshot = CaptureSystemMetrics();
        _service.StopMeasurement(_seederName, memoryUsageBytes: memoryUsage, itemCount: 0, finalSnapshot: finalSnapshot);
        _disposed = true;
    }
    
    /// <summary>
    /// Captures a snapshot of system metrics.
    /// </summary>
    /// <returns>A system metrics snapshot.</returns>
    private static SystemMetricsSnapshot CaptureSystemMetrics()
    {
        var process = Process.GetCurrentProcess();
        process.Refresh();
        
        return new SystemMetricsSnapshot
        {
            Timestamp = DateTime.UtcNow,
            WorkingSetMemory = process.WorkingSet64,
            PrivateMemory = process.PrivateMemorySize64,
            VirtualMemory = process.VirtualMemorySize64,
            CpuTime = process.TotalProcessorTime,
            Threads = process.Threads.Count,
            Handles = process.HandleCount
        };
    }
}