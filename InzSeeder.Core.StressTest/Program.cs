using InzSeeder.Core.Contracts;
using InzSeeder.Core.Extensions;
using InzSeeder.Core.Models;
using InzSeeder.Core.StressTest.Data;
using InzSeeder.Core.StressTest.Models;
using InzSeeder.Core.StressTest.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.StressTest;

class Program
{
    static async Task Main(string[] args)
    {
        // Set the environment variable
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

        // Parse command line arguments
        var datasetSize = ParseDatasetSizeArgument(args);

        // Create the host
        var builder = Host.CreateApplicationBuilder(args);

        // Configure logging to suppress EF Core database commands
        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.SetMinimumLevel(LogLevel.Information);
        // Suppress EF Core database command logs
        builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

        // Configure the database context (using SQLite for stress testing)
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "Data");
        if (!Directory.Exists(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }

        var dbPath = Path.Combine(dataDirectory, "stress_test.db");
        builder.Services.AddDbContext<StressTestDbContext>(options => { options.UseSqlite($"Data Source={dbPath}"); });

        // Configure stress test settings
        var stressTestSettings = new StressTestConfiguration
        {
            DatasetSize = datasetSize, // Use parsed dataset size
            BatchSize = 5000,
            EnableDetailedMetrics = true,
            ReportFormat = StressTestReportFormat.ConsoleAndFile,
            Iterations = 1
        };

        // Configure seeding settings
        var seedingSettings = new SeederConfiguration
        {
            Profile = new SeedingProfile
            {
                StrictMode = false
            },
            BatchSettings = new SeederBatchSettings
            {
                DefaultBatchSize = stressTestSettings.BatchSize
            }
        };

        // Add the seeder services with stress test configuration
        builder.Services.AddInzSeeder(seedingSettings)
            .UseDbContext<StressTestDbContext>()
            .RegisterEntitySeedersFromAssemblies(typeof(Program).Assembly);

        // Use file-based seed data provider instead of embedded resources
        var seedDataDirectory = Path.Combine(AppContext.BaseDirectory, "SeedData");
        builder.Services.AddSingleton<ISeedDataProvider>(_ => new FileSeedDataProvider(seedDataDirectory));

        // Add stress test services
        builder.Services.AddSingleton(stressTestSettings);
        builder.Services.AddSingleton<StressTestDataGenerator>();
        builder.Services.AddSingleton<StressTestMetricsCollector>();
        builder.Services.AddSingleton<StressTestReporter>();
        builder.Services.AddSingleton<StressTestRunner>();

        var host = builder.Build();

        // Ensure the database is created
        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<StressTestDbContext>();
            await dbContext.Database.EnsureCreatedAsync();
        }

        // Run the stress test
        using (var scope = host.Services.CreateScope())
        {
            var stressTestRunner = scope.ServiceProvider.GetRequiredService<StressTestRunner>();
            await stressTestRunner.RunStressTestAsync();
        }

        // Clean up the database files
        try
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);

            // Clean up SQLite WAL and SHM files
            var dbShmPath = Path.Combine(dataDirectory, "stress_test.db-shm");
            if (File.Exists(dbShmPath)) File.Delete(dbShmPath);

            var dbWalPath = Path.Combine(dataDirectory, "stress_test.db-wal");
            if (File.Exists(dbWalPath)) File.Delete(dbWalPath);

            Console.WriteLine("Database files cleaned up successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to clean up database files: {ex.Message}");
        }
    }

    /// <summary>
    /// Parses the dataset size from command line arguments.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>The parsed dataset size.</returns>
    private static StressTestDatasetSize ParseDatasetSizeArgument(string[] args)
    {
        if (args.Length <= 0) return StressTestDatasetSize.Medium;

        if (Enum.TryParse<StressTestDatasetSize>(args[0], true, out var datasetSize))
        {
            return datasetSize;
        }

        Console.WriteLine($"Invalid dataset size '{args[0]}'. Using Medium size.");

        return StressTestDatasetSize.Medium;
    }
}