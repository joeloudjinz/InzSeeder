# InzSeeder Sample Project

This is a complete example project demonstrating how to use the InzSeeder package in a .NET application with an in-memory database.

## Project Structure

```
InzSeeder.Samples.InMemory/
├── Program.cs
├── InzSeeder.Samples.InMemory.csproj
├── Data/
│   └── AppDbContext.cs
├── Models/
│   ├── Product.cs
│   └── ProductSeedModel.cs
├── Seeders/
│   └── ProductSeeder.cs
└── SeedData/
    └── products.json
```

## How to Run

1. Navigate to the InzSeeder.Samples.InMemory directory:
   ```bash
   cd InzSeeder.Samples.InMemory
   ```

2. Run the project:
   ```bash
   dotnet run
   ```

This will seed the in-memory database with the sample product data.

## Implementation Details

### Program.cs

The main entry point demonstrates how to configure and use InzSeeder with the unified fluent API:

```csharp
using InzSeeder.Core.Extensions;
using InzSeeder.Core.Models;
using InzSeeder.Core.Utilities;
using InzSeeder.Samples.InMemory.Data;
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
            options.UseInMemoryDatabase("SampleDb")
                   .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
        });

        // Create seeding settings
        var seedingSettings = new SeedingSettings
        {
            Environment = "Development",
            Profiles = new Dictionary<string, SeedingProfile>
            {
                ["Development"] = new()
                {
                    EnabledSeeders = ["products"],
                    StrictMode = false
                }
            },
            BatchSettings = new SeederBatchSettings
            {
                DefaultBatchSize = 100
            }
        };

        // Determine environment
        EnvironmentUtility.DetermineEnvironment("Development");

        // Add the seeder services with external configuration using fluent API
        builder.Services.AddInzSeeder(seedingSettings)
            .UseDbContext<AppDbContext>()
            .RegisterEntitySeedersFromAssemblies();

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
```

### Key Features Demonstrated

1. **Fluent API Configuration**: Uses the unified `AddInzSeeder().UseDbContext().RegisterEntitySeedersFromAssemblies()` pattern
2. **Programmatic Configuration**: All settings are provided programmatically rather than through appsettings.json files
3. **Automatic Seeder Discovery**: Seeders are automatically discovered and registered
4. **Existing DbContext Integration**: Shows how to integrate with an existing DbContext using `UseDbContext<TContext>()`

## Seed Data

The sample includes a JSON seed data file in `SeedData/products.json`:

```json
[
  {
    "id": 1,
    "name": "Laptop",
    "price": 999.99
  },
  {
    "id": 2,
    "name": "Mouse",
    "price": 29.99
  },
  {
    "id": 3,
    "name": "Keyboard",
    "price": 79.99
  }
]
```

## Using InzSeeder in Your Own Project

To integrate InzSeeder into your own project, follow this pattern:

```csharp
// Create seeding settings
var seedingSettings = new SeedingSettings
{
    Environment = "Development",
    Profiles = new Dictionary<string, SeedingProfile>
    {
        ["Development"] = new SeedingProfile
        {
            EnabledSeeders = ["products", "users", "roles"],
            StrictMode = false
        }
    },
    BatchSettings = new SeederBatchSettings
    {
        DefaultBatchSize = 100
    }
};

// Register the seeder with configuration using fluent API
services.AddInzSeeder(seedingSettings)
    .UseDbContext<YourDbContext>()
    .RegisterEntitySeedersFromAssemblies();
```

This approach provides a clean, unified way to configure InzSeeder without requiring appsettings.json files.