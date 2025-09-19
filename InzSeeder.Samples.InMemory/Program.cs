using InzSeeder.Core.Extensions;
using InzSeeder.Core.Models;
using InzSeeder.Samples.InMemory.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InzSeeder.Samples.InMemory;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        // or Environment.SetEnvironmentVariable("SEEDING_ENVIRONMENT", "Development");

        // Configure the database context
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            // Using in-memory database for this example
            options.UseInMemoryDatabase("SampleDb").ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning));
        });

        // Create seeding settings
        var seedingSettings = new SeederConfiguration
        {
            Profile = new SeedingProfile
            {
                EnabledSeeders = ["products"],
                StrictMode = false
            },
            BatchSettings = new SeederBatchSettings
            {
                DefaultBatchSize = 100
            }
        };

        // Add the seeder services with external configuration using fluent API
        builder.Services.AddInzSeeder(seedingSettings)
            .UseDbContext<AppDbContext>()
            .RegisterEntitySeedersFromAssemblies(typeof(Program).Assembly)
            .RegisterEmbeddedSeedDataFromAssemblies(typeof(Program).Assembly);

        var host = builder.Build();

        // Run the seeder
        using (var scope = host.Services.CreateScope())
        {
            await scope.ServiceProvider.RunInzSeeder(CancellationToken.None);
        }

        Console.WriteLine("Seeding completed successfully!");
    }
}