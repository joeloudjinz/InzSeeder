using InzSeeder.Core.Extensions;
using InzSeeder.Core.StressTest.Data;
using InzSeeder.Core.StressTest.Models;
using InzSeeder.Core.StressTest.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.StressTest;

/// <summary>
/// Runner for executing stress tests on the seeding process.
/// </summary>
public class StressTestRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly StressTestConfiguration _configuration;
    private readonly StressTestDataGenerator _dataGenerator;
    private readonly StressTestMetricsCollector _metricsCollector;
    private readonly StressTestReporter _reporter;
    private readonly ILogger<StressTestRunner> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="StressTestRunner"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="configuration">The stress test configuration.</param>
    /// <param name="dataGenerator">The data generator service.</param>
    /// <param name="metricsCollector">The metrics collector service.</param>
    /// <param name="reporter">The reporting service.</param>
    /// <param name="logger">The logger.</param>
    public StressTestRunner(
        IServiceProvider serviceProvider,
        StressTestConfiguration configuration,
        StressTestDataGenerator dataGenerator,
        StressTestMetricsCollector metricsCollector,
        StressTestReporter reporter,
        ILogger<StressTestRunner> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _dataGenerator = dataGenerator;
        _metricsCollector = metricsCollector;
        _reporter = reporter;
        _logger = logger;
    }

    /// <summary>
    /// Runs the stress test.
    /// </summary>
    public async Task RunStressTestAsync()
    {
        _logger.LogInformation("Starting stress test with dataset size: {DatasetSize}", _configuration.DatasetSize);
        
        // Generate seed data files based on configuration
        await GenerateSeedDataFilesAsync();
        
        var iterationMetrics = new List<StressTestIterationMetrics>();
        
        // Run multiple iterations if configured
        for (var i = 0; i < _configuration.Iterations; i++)
        {
            _logger.LogInformation("Starting iteration {Iteration} of {TotalIterations}", i + 1, _configuration.Iterations);
            
            // Clear database between iterations if configured
            if (i > 0 && _configuration.ClearBetweenIterations)
            {
                await ClearDatabaseAsync();
            }
            
            // Run the seeding process
            var iterationMetric = await RunSeedingIterationAsync(i);
            iterationMetrics.Add(iterationMetric);
        }
        
        // Clean up generated seed data files
        await CleanupSeedDataFilesAsync();
        
        // Generate final report
        await _reporter.GenerateReportAsync(iterationMetrics, _metricsCollector.GetSummaryMetrics());
        
        _logger.LogInformation("Stress test completed successfully");
    }

    /// <summary>
    /// Generates seed data files for the stress test based on configuration.
    /// </summary>
    private async Task GenerateSeedDataFilesAsync()
    {
        _logger.LogInformation("Generating seed data files with dataset size: {DatasetSize}", _configuration.DatasetSize);
        
        var datasetSize = (int)_configuration.DatasetSize;
        var relatedEntitiesCount = Math.Max(1, datasetSize / 10); // 10% of main entities
        
        // Generate related entities data
        var relatedEntities = _dataGenerator.GenerateRelatedTestEntities(relatedEntitiesCount).ToList();
        await WriteSeedDataToFileAsync("related-test-entities", relatedEntities);
        
        // Generate main entities data
        var testEntities = _dataGenerator.GenerateTestEntities(datasetSize).ToList();
        await WriteSeedDataToFileAsync("test-entities", testEntities);
        
        _logger.LogInformation("Generated {RelatedCount} related entities and {EntityCount} test entities", 
            relatedEntitiesCount, datasetSize);
    }

    /// <summary>
    /// Writes seed data to a JSON file.
    /// </summary>
    /// <param name="fileName">The name of the file.</param>
    /// <param name="data">The data to write.</param>
    private static async Task WriteSeedDataToFileAsync<T>(string fileName, IEnumerable<T> data)
    {
        var seedDataDirectory = Path.Combine(AppContext.BaseDirectory, "SeedData");
        if (!Directory.Exists(seedDataDirectory))
        {
            Directory.CreateDirectory(seedDataDirectory);
        }
        
        var filePath = Path.Combine(seedDataDirectory, $"{fileName}.json");
        var json = System.Text.Json.JsonSerializer.Serialize(data, new System.Text.Json.JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
        
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Cleans up generated seed data files.
    /// </summary>
    private async Task CleanupSeedDataFilesAsync()
    {
        _logger.LogInformation("Cleaning up generated seed data files");
        
        var seedDataDirectory = Path.Combine(AppContext.BaseDirectory, "SeedData");
        if (Directory.Exists(seedDataDirectory))
        {
            var relatedEntitiesFile = Path.Combine(seedDataDirectory, "related-test-entities.json");
            var testEntitiesFile = Path.Combine(seedDataDirectory, "test-entities.json");
            
            if (File.Exists(relatedEntitiesFile))
            {
                try
                {
                    File.Delete(relatedEntitiesFile);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete related entities seed data file");
                }
            }
            
            if (File.Exists(testEntitiesFile))
            {
                try
                {
                    File.Delete(testEntitiesFile);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete test entities seed data file");
                }
            }
            
            // Try to remove the directory if it's empty
            try
            {
                if (Directory.GetFiles(seedDataDirectory).Length == 0 && Directory.GetDirectories(seedDataDirectory).Length == 0)
                {
                    Directory.Delete(seedDataDirectory);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete SeedData directory");
            }
        }
        
        _logger.LogInformation("Seed data files cleanup completed");
    }

    /// <summary>
    /// Clears the database.
    /// </summary>
    private async Task ClearDatabaseAsync()
    {
        _logger.LogInformation("Clearing database");
        
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<StressTestDbContext>();
        
        // Remove all entities
        dbContext.TestEntities.RemoveRange(dbContext.TestEntities);
        dbContext.RelatedTestEntities.RemoveRange(dbContext.RelatedTestEntities);
        
        await dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Runs a single iteration of the seeding process.
    /// </summary>
    /// <param name="iteration">The iteration number.</param>
    private async Task<StressTestIterationMetrics> RunSeedingIterationAsync(int iteration)
    {
        _logger.LogInformation("Running seeding iteration {Iteration}", iteration + 1);
        
        var startTime = DateTime.UtcNow;
        
        // Start stress test measurement
        using var stressToken = _metricsCollector.StartMeasurement($"iteration-{iteration}");
        
        // Run the seeding process
        using var scope = _serviceProvider.CreateScope();
        await scope.ServiceProvider.RunInzSeeder(default);
        
        var endTime = DateTime.UtcNow;
        
        return new StressTestIterationMetrics
        {
            Iteration = iteration + 1,
            StartTime = startTime,
            EndTime = endTime,
            Duration = endTime - startTime
        };
    }
}

/// <summary>
/// Represents metrics for a single iteration of the stress test.
/// </summary>
public class StressTestIterationMetrics
{
    /// <summary>
    /// Gets or sets the iteration number.
    /// </summary>
    public int Iteration { get; set; }
    
    /// <summary>
    /// Gets or sets the start time of the iteration.
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Gets or sets the end time of the iteration.
    /// </summary>
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// Gets or sets the duration of the iteration.
    /// </summary>
    public TimeSpan Duration { get; set; }
}