namespace InzSeeder.Core.StressTest.Models;

/// <summary>
/// Configuration for stress testing the seeding process.
/// </summary>
public class StressTestConfiguration
{
    /// <summary>
    /// Gets or sets the size of the dataset to generate for testing.
    /// </summary>
    public StressTestDatasetSize DatasetSize { get; set; } = StressTestDatasetSize.Medium;
    
    /// <summary>
    /// Gets or sets the batch size for processing records.
    /// </summary>
    public int BatchSize { get; set; } = 100;
    
    /// <summary>
    /// Gets or sets whether to collect detailed metrics.
    /// </summary>
    public bool EnableDetailedMetrics { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the format for the test report.
    /// </summary>
    public StressTestReportFormat ReportFormat { get; set; } = StressTestReportFormat.Console;
    
    /// <summary>
    /// Gets or sets the number of iterations to run the test.
    /// </summary>
    public int Iterations { get; set; } = 1;
    
    /// <summary>
    /// Gets or sets whether to clear the database between iterations.
    /// </summary>
    public bool ClearBetweenIterations { get; set; } = true;
}

/// <summary>
/// Represents the size of the dataset to generate for stress testing.
/// </summary>
public enum StressTestDatasetSize
{
    /// <summary>
    /// Small dataset (1K records).
    /// </summary>
    Small = 1000,
    
    /// <summary>
    /// Medium dataset (10K records).
    /// </summary>
    Medium = 10000,
    
    /// <summary>
    /// Large dataset (100K records).
    /// </summary>
    Large = 100000,
    
    /// <summary>
    /// Extra large dataset (1M records).
    /// </summary>
    ExtraLarge = 1000000
}

/// <summary>
/// Represents the format for the stress test report.
/// </summary>
public enum StressTestReportFormat
{
    /// <summary>
    /// Output report to console only.
    /// </summary>
    Console,
    
    /// <summary>
    /// Output report to file only.
    /// </summary>
    File,
    
    /// <summary>
    /// Output report to both console and file.
    /// </summary>
    ConsoleAndFile
}