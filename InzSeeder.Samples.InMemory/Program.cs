using InzSeeder.Core.Contracts;
using InzSeeder.Core.Extensions;
using InzSeeder.Samples.InMemory.Data;
using InzSeeder.Samples.InMemory.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InzSeeder.Samples.InMemory;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        // Configure the database context
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            // Using in-memory database for this example
            options.UseInMemoryDatabase("SampleDb");
        });

        // Add the seeder services
        builder.Services.AddInzSeeder();

        // Register our custom seeder
        builder.Services.AddScoped<IEntitySeeder, ProductSeeder>();

        var host = builder.Build();

        // Run the seeder
        using (var scope = host.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<ISeedingOrchestrator>();
            await seeder.SeedDataAsync(CancellationToken.None);
        }

        Console.WriteLine("Seeding completed successfully!");
    }
}