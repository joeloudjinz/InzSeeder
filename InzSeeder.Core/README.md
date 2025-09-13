# InzSeeder

InzSeeder is a flexible, generic data seeding library for .NET applications that can be used to seed any database with initial data.

## Features

- **Generic Design**: Works with any Entity Framework Core DbContext
- **Idempotent Seeding**: Safe to run multiple times without creating duplicates
- **Environment-Aware**: Supports environment-specific seeding configurations
- **Dependency Management**: Handles dependencies between seeders
- **Batch Processing**: Processes large datasets in batches for better performance
- **Audit Logging**: Tracks all seeding operations
- **Hash-Based Change Detection**: Only re-seeds when data changes
- **Extensible Architecture**: Easy to create custom seeders
- **Command-Line Interface**: Rich CLI with preview, dry-run, and health check modes

## Installation

Add the InzSeeder NuGet package to your project:

```bash
dotnet add package InzSeeder
```

## Quick Start

1. Create a custom seeder by inheriting from `BaseEntitySeeder`:

```csharp
public class ProductSeeder : BaseEntitySeeder<Product, ProductSeedModel>
{
    public override string SeedName => "products";

    protected override object GetBusinessKeyFromEntity(Product entity) => entity.Id;
    
    protected override object GetBusinessKey(ProductSeedModel model) => model.Id;
    
    protected override Product MapToEntity(ProductSeedModel model)
    {
        return new Product
        {
            Id = model.Id,
            Name = model.Name,
            Price = model.Price
        };
    }
    
    protected override void UpdateEntity(Product existingEntity, ProductSeedModel model)
    {
        existingEntity.Name = model.Name;
        existingEntity.Price = model.Price;
    }
}
```

2. Register the seeder in your application with configuration using the fluent API:

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

// Register the seeder services with configuration using fluent API
services.AddInzSeeder(seedingSettings)
    .UseDbContext<YourDbContext>()
    .RegisterEntitySeedersFromAssemblies();
```

3. Add JSON seed data to the `SeedData` folder in your library project:

```json
[
  {
    "id": 1,
    "name": "Product 1",
    "price": 19.99
  }
]
```

4. Run the seeder:

```bash
dotnet run --project InzSeeder.Core
```

## Configuration

InzSeeder now accepts configuration externally rather than requiring appsettings.json files. You can configure the seeder programmatically when calling `AddInzSeeder()`:

```csharp
var seedingSettings = new SeedingSettings
{
    Environment = "Development",
    Profiles = new Dictionary<string, SeedingProfile>
    {
        ["Development"] = new SeedingProfile
        {
            EnabledSeeders = ["products"],
            StrictMode = false
        },
        ["Production"] = new SeedingProfile
        {
            EnabledSeeders = ["roles"],
            StrictMode = true
        }
    },
    BatchSettings = new SeederBatchSettings
    {
        DefaultBatchSize = 100,
        SeederBatchSizes = new Dictionary<string, int>
        {
            ["users"] = 50,
            ["roles"] = 10
        }
    }
};

services.AddInzSeeder(seedingSettings)
    .UseDbContext<YourDbContext>()
    .RegisterEntitySeedersFromAssemblies();
```

## Examples

See the `InzSeeder.Samples.InMemory` directory for a complete example of how to use the InzSeeder package in a .NET application.

## Testing

Run the tests with:

```bash
dotnet test
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.