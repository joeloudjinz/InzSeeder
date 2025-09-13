using InzSeeder.Core.Contracts;
using InzSeeder.Core.Extensions;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Core.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using static System.Console;

namespace InzSeeder.Core;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var argumentParser = new CommandLineArgumentParser();
        var commandLineArgs = argumentParser.Parse(args);

        // Show help if requested
        if (args.Contains("--help") || args.Contains("-h"))
        {
            argumentParser.ShowHelp();
            return 0;
        }

        EnvironmentUtility.DetermineEnvironment(commandLineArgs.Environment);

        try
        {
            var builder = new HostBuilder();

            builder.UseContentRoot(Directory.GetCurrentDirectory());

            // Set the environment for the host.
            builder.UseEnvironment(EnvironmentUtility.Environment());

            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            });

            builder.ConfigureServices((context, services) =>
            {
                // Now that configuration is fully built, we can configure our services.
                ConfigureServices(services, context.Configuration);
            });

            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
            });

            using var host = builder.Build();
            using var scope = host.Services.CreateScope();
            return await Run(scope.ServiceProvider, commandLineArgs);
        }
        catch (Exception ex)
        {
            await Error.WriteLineAsync($"Seeding failed: {ex.Message}");
            await Error.WriteLineAsync($"Stack trace: {ex.StackTrace}");
            return 1;
        }
    }

    private static async Task<int> Run(IServiceProvider services, SeederCommandLineArgs commandLineArgs)
    {
        // Run health checks if requested
        if (commandLineArgs.HealthCheck)
        {
            var healthCheckService = services.GetRequiredService<SeederHealthCheckService>();
            var result = await healthCheckService.PerformHealthCheckAsync(CancellationToken.None);
            return result ? 0 : 1;
        }

        // Run purge if requested
        if (commandLineArgs.Purge)
        {
            var purgeService = services.GetRequiredService<SeederPurgeService>();
            var result = await purgeService.PurgeAsync(commandLineArgs.Yes, CancellationToken.None);
            return result ? 0 : 1;
        }

        var configuration = services.GetRequiredService<IConfiguration>();
        // Try to get settings from configuration, but it's not required since we're using external configuration
        var settings = configuration.GetSection(SeederConfiguration.SectionName).Get<SeederConfiguration>() ?? new SeederConfiguration();
        if (commandLineArgs.Preview)
        {
            var previewService = services.GetRequiredService<ExecutionPlanPreviewService>();
            await previewService.ShowPreviewAsync(settings, CancellationToken.None);
            return 0;
        }

        if (commandLineArgs.DryRun)
        {
            WriteLine("[Not Implemented] Dry run mode: Showing what would be executed without running...");
            // TODO implement DryRun command handler
            // Console.WriteLine("Dry run completed. No seeders were executed.");
            return 0;
        }

        if (!EnvironmentUtility.ValidateConfiguration(settings))
        {
            await Error.WriteLineAsync("Invalid seeding configuration detected. Please check your configuration and try again.");
            return 1;
        }

        WriteLine("Starting database seeding process...");

        // Run the seeder
        var seeder = services.GetRequiredService<ISeedingOrchestrator>();
        await seeder.SeedDataAsync(CancellationToken.None);

        WriteLine("Seeding completed successfully.");
        return 0;
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register seeder services using the extension method with default settings
        // Users can provide their own settings when calling AddInzSeeder in their applications
        services.AddInzSeeder();
    }
}