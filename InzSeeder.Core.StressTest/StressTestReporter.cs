using System.Text;
using InzSeeder.Core.StressTest.Models;
using InzSeeder.Core.StressTest.Services;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.StressTest;

/// <summary>
/// Service for generating reports from stress test results.
/// </summary>
public class StressTestReporter
{
    private readonly ILogger<StressTestReporter> _logger;
    private readonly StressTestConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="StressTestReporter"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration"></param>
    public StressTestReporter(ILogger<StressTestReporter> logger, StressTestConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Generates a report from stress test results.
    /// </summary>
    /// <param name="iterationMetrics">The iteration metrics.</param>
    /// <param name="summaryMetrics">The summary metrics.</param>
    public async Task GenerateReportAsync(ICollection<StressTestIterationMetrics> iterationMetrics, StressTestSummaryMetrics summaryMetrics)
    {
        var report = new StringBuilder();
        report.AppendLine($"=== InzSeeder Stress Test Report - {DateTimeOffset.Now.ToString("F")} ===");
        report.AppendLine($"Data Size: {_configuration.DatasetSize.ToString()}");
        report.AppendLine();

        // Add test summary
        report.AppendLine("Test Summary:");
        report.AppendLine($"Total Iterations: {iterationMetrics.Count}");
        report.AppendLine($@"Total Duration: {summaryMetrics.Duration:mm\:ss\.fff}");
        report.AppendLine($"Peak Memory Usage: {FormatBytes(summaryMetrics.PeakWorkingSetMemory)}");
        report.AppendLine($@"Total CPU Time: {summaryMetrics.TotalCpuTime:mm\:ss\.fff}");
        report.AppendLine($"Average Thread Count: {summaryMetrics.AverageThreadCount}");
        report.AppendLine("-------------");
        report.AppendLine();

        // Add iteration details
        report.AppendLine("Iteration Details:");
        foreach (var iteration in iterationMetrics) report.AppendLine($@"Iteration {iteration.Iteration}: {iteration.Duration:mm\:ss\.fff}");
        report.AppendLine("-----------------");
        report.AppendLine();

        // Add system metrics
        report.AppendLine("System Metrics:");
        report.AppendLine($"Peak Working Set Memory: {FormatBytes(summaryMetrics.PeakWorkingSetMemory)}");
        report.AppendLine($"Peak Private Memory: {FormatBytes(summaryMetrics.PeakPrivateMemory)}");
        report.AppendLine($"Peak Virtual Memory: {FormatBytes(summaryMetrics.PeakVirtualMemory)}");
        report.AppendLine($@"Total CPU Time: {summaryMetrics.TotalCpuTime:mm\:ss\.fff}");
        report.AppendLine($"Average Thread Count: {summaryMetrics.AverageThreadCount}");
        report.AppendLine($"Average Handle Count: {summaryMetrics.AverageHandleCount}");
        report.AppendLine("---------------");
        report.AppendLine();

        var reportContent = report.ToString();

        // Output to console
        Console.WriteLine(reportContent);

        // Write to file
        var fileName = $"stress-test-report-{DateTime.UtcNow:yyyyMMdd-HHmmss}.txt";
        var filePath = Path.Combine(AppContext.BaseDirectory, fileName);
        await File.WriteAllTextAsync(filePath, reportContent);

        _logger.LogInformation("Stress test report generated: {FilePath}", filePath);
    }

    /// <summary>
    /// Formats bytes into a human-readable string.
    /// </summary>
    /// <param name="bytes">The number of bytes.</param>
    /// <returns>A formatted string.</returns>
    private static string FormatBytes(long bytes)
    {
        var units = new[] { "B", "KB", "MB", "GB", "TB" };
        var unitIndex = 0;
        var value = (double)bytes;

        while (value >= 1024 && unitIndex < units.Length - 1)
        {
            value /= 1024;
            unitIndex++;
        }

        return $"{value:F2} {units[unitIndex]}";
    }
}