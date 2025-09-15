using InzSeeder.Core.Extensions;
using InzSeeder.Core.Models;
using InzSeeder.Core.Utilities;
using InzSeeder.Samples.SQLite.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InzSeeder.Samples.SQLite;

internal class Program
{
    static async Task Main(string[] args)
    {
        // Determine environment from command line args or environment variables
        var environment = DetermineEnvironment(args);

        // Initialize the environment utility
        EnvironmentUtility.DetermineEnvironment(environment);

        Console.WriteLine($"Running InzSeeder SQLite Sample in {environment} environment");

        // Create host builder
        var builder = Host.CreateApplicationBuilder(args);
        builder.Environment.EnvironmentName = environment;

        // Add configuration
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

        // Configure services
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Create seeding configuration
        var seedingSettings = new SeederConfiguration
        {
            Environment = environment,
            Profiles = new Dictionary<string, SeedingProfile>
            {
                ["Development"] = new()
                {
                    EnabledSeeders = ["products", "users", "categories", "product-categories"],
                    StrictMode = false
                },
                ["Test"] = new()
                {
                    EnabledSeeders = ["users"],
                    StrictMode = true
                },
                ["Production"] = new()
                {
                    EnabledSeeders = ["products", "categories"],
                    StrictMode = true
                }
            },
            BatchSettings = new SeederBatchSettings
            {
                DefaultBatchSize = 50,
                SeederBatchSizes = new Dictionary<string, int>
                {
                    ["users"] = 25,
                    ["products"] = 10
                }
            }
        };

        // Register the seeder services with configuration
        builder.Services.AddInzSeeder(seedingSettings)
            .UseDbContext<ApplicationDbContext>()
            .RegisterEntitySeedersFromAssemblies(typeof(Program).Assembly)
            .RegisterEmbeddedSeedDataFromAssemblies(typeof(Program).Assembly);

        var host = builder.Build();

        // Ensure the database is created
        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            // Ensure the database schema is created
            var created = await dbContext.Database.EnsureCreatedAsync();
            if (created)
            {
                Console.WriteLine("Database created successfully.");
            }
            else
            {
                Console.WriteLine("Database already exists.");
            }

            // Run the seeders
            await scope.ServiceProvider.RunInzSeeder(CancellationToken.None);
        }

        Console.WriteLine("Seeding completed successfully!");

        // Display the seeded data
        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await DisplaySeededData(dbContext);
        }
    }

    static string DetermineEnvironment(string[] args)
    {
        // Check command line arguments
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--environment" && i + 1 < args.Length)
            {
                return args[i + 1];
            }

            if (args[i].StartsWith("--environment="))
            {
                return args[i].Substring(14); // --environment= is 14 characters
            }
        }

        // Check environment variables
        var seedingEnv = Environment.GetEnvironmentVariable("SEEDING_ENVIRONMENT");
        if (!string.IsNullOrEmpty(seedingEnv))
            return seedingEnv;

        var dotnetEnv = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        if (!string.IsNullOrEmpty(dotnetEnv))
            return dotnetEnv;

        // Default to Development
        return "Development";
    }

    static async Task DisplaySeededData(ApplicationDbContext dbContext)
    {
        Console.WriteLine("\n--- Seeded Data ---");

        var products = await dbContext.Products.ToListAsync();
        Console.WriteLine($"\nProducts ({products.Count} total):");
        foreach (var product in products.Take(5))
        {
            Console.WriteLine($"  - {product.Id}: {product.Name} (${product.Price})");
        }

        if (products.Count > 5)
            Console.WriteLine($"  ... and {products.Count - 5} more");

        var users = await dbContext.Users.ToListAsync();
        Console.WriteLine($"\nUsers ({users.Count} total):");
        foreach (var user in users.Take(5))
        {
            Console.WriteLine($"  - {user.Id}: {user.FirstName} {user.LastName} ({user.Email})");
        }

        if (users.Count > 5)
            Console.WriteLine($"  ... and {users.Count - 5} more");

        var categories = await dbContext.Categories.ToListAsync();
        Console.WriteLine($"\nCategories ({categories.Count} total):");
        foreach (var category in categories)
        {
            Console.WriteLine($"  - {category.Id}: {category.Name} (Slug: {category.Slug}, Active: {category.IsActive})");
        }
    }
}