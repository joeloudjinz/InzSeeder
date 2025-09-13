using InzSeeder.Core.Models;
using static System.Console;

namespace InzSeeder.Core.Services;

/// <summary>
/// Service for parsing command-line arguments.
/// </summary>
public class CommandLineArgumentParser
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommandLineArgumentParser"/> class.
    /// </summary>
    public CommandLineArgumentParser()
    {
    }

    /// <summary>
    /// Parses command-line arguments into a <see cref="SeederCommandLineArgs"/> object.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <returns>The parsed command-line arguments.</returns>
    public SeederCommandLineArgs Parse(string[] args)
    {
        var result = new SeederCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            switch (arg)
            {
                case "--environment":
                    if (i + 1 < args.Length)
                    {
                        result.Environment = args[++i];
                    }
                    else
                    {
                        WriteLine("Missing value for --environment argument");
                    }

                    break;

                case "--force":
                    result.Force = [];
                    // Collect all following arguments until we hit another flag
                    while (i + 1 < args.Length && !args[i + 1].StartsWith("--"))
                    {
                        result.Force.Add(args[++i]);
                    }

                    break;

                case "--dry-run":
                    result.DryRun = true;
                    break;

                case "--preview":
                    result.Preview = true;
                    break;

                case "--unsafe":
                    result.Unsafe = true;
                    break;

                case "--health-check":
                    result.HealthCheck = true;
                    break;

                case "--purge":
                    result.Purge = true;
                    break;

                case "-y":
                case "--yes":
                    result.Yes = true;
                    break;

                case "--help":
                case "-h":
                    // Help is handled in the main program, but we still need to recognize it
                    // to avoid the "Unknown argument" error
                    break;

                default:
                    Error.WriteLine("Unknown argument: {0}", arg);
                    break;
            }
        }

        return result;
    }

    /// <summary>
    /// Displays help information for the command-line arguments.
    /// </summary>
    public void ShowHelp()
    {
        WriteLine("Inz.Seeder");
        WriteLine("Important:");
        WriteLine("Environment must be specified, the seeder will use the value of --environment if specified, or the environment variable SEEDING_ENVIRONMENT, or environment variable DOTNET_ENVIRONMENT.");
        WriteLine();
        WriteLine("Usage: dotnet run --project Inz.Seeder [options]");
        WriteLine();
        WriteLine("Options:");
        WriteLine("  --environment <name>    Override environment detection");
        WriteLine("  --force <seeders>       Force run specific seeders (space-separated list)");
        WriteLine("  --preview               Show detailed execution plan");
        WriteLine("  --unsafe                Override safety checks");
        WriteLine("  --health-check          Run simple DB connection health checks");
        WriteLine("  --purge                 Remove all existing records from the database (NOT ALLOWED in Production)");
        WriteLine("  -y, --yes               Bypass confirmation for the purge operation");
        WriteLine("  --dry-run               [NOT IMPLEMENTED] Show what would be executed without running");
        WriteLine();
        WriteLine("Examples:");
        WriteLine("  dotnet run --project Inz.Seeder --environment Production");
        WriteLine("  dotnet run --project Inz.Seeder --force users roles");
        WriteLine("  dotnet run --project Inz.Seeder --dry-run");
        WriteLine("  dotnet run --project Inz.Seeder --preview");
        WriteLine("  dotnet run --project Inz.Seeder --unsafe --environment Production");
        WriteLine("  dotnet run --project Inz.Seeder --health-check");
        WriteLine("  dotnet run --project Inz.Seeder --purge --yes");
        WriteLine();
        WriteLine("Note: The --purge command is NOT ALLOWED in Production environment.");
    }
}