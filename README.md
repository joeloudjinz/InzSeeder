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

## Projects

This solution contains the following projects:

### InzSeeder.Core
The main library implementation.

### InzSeeder.Samples.InMemory
A sample project demonstrating usage with an in-memory database.

### InzSeeder.Samples.SQLite
A sample project demonstrating usage with a SQLite database.

## Quick Start

To get started with InzSeeder, you can either:

1. Add the NuGet package to your project:
   ```bash
   dotnet add package InzSeeder
   ```

2. Or clone this repository and explore the sample projects:
   ```bash
   git clone <repository-url>
   cd InzSeeder
   ```

## Detailed Usage Guide

### 1. Installation

Add the InzSeeder NuGet package to your project:

```bash
dotnet add package InzSeeder
```

### 2. Create Your Data Models

First, create your entity models that represent the data you want to seed:

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### 3. Create Seed Data Models

Create corresponding seed data models that match your JSON data structure:

```csharp
public class ProductSeedModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

### 4. Create Custom Seeders

Create custom seeders by inheriting from `BaseEntitySeeder<TEntity, TModel>`:

```csharp
public class ProductSeeder : BaseEntitySeeder<Product, ProductSeedModel>
{
    public ProductSeeder(
        ISeedDataProvider seedDataProvider,
        ISeederDbContext dbContext,
        ILogger<ProductSeeder> logger,
        SeederConfiguration? seedingSettings = null,
        SeedingPerformanceMetricsService? performanceMetricsService = null
    ) : base(seedDataProvider, dbContext, logger, seedingSettings, performanceMetricsService)
    {
    }

    public override string SeedName => "products";

    // You can use other properties of different type based on your choice, like `string Key` 
    protected override object GetBusinessKeyFromEntity(Product entity) => entity.Id;

    // You can use other properties of different type based on your choice, like `string Key`
    protected override object GetBusinessKey(ProductSeedModel model) => model.Id;

    protected override Product MapToEntity(ProductSeedModel model)
    {
        return new Product
        {
            Id = model.Id,
            Name = model.Name,
            Price = model.Price,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    protected override void UpdateEntity(Product existingEntity, ProductSeedModel model)
    {
        existingEntity.Name = model.Name;
        existingEntity.Price = model.Price;
        existingEntity.UpdatedAt = DateTime.UtcNow;
    }
}
```

### 5. Prepare Seed Data

Create JSON files in a `SeedData` folder in your project:

```json
// SeedData/products.json
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

Configure your project to embed these files as resources:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <!-- Your project properties -->
  </PropertyGroup>
  
  <ItemGroup>
    <EmbeddedResource Include="SeedData\**\*.json" />
  </ItemGroup>
</Project>
```

### 6. Configure Services

In your application's startup code (Program.cs or Startup.cs), configure the InzSeeder services:

```csharp
using InzSeeder.Core.Extensions;
using InzSeeder.Core.Models;

var builder = Host.CreateApplicationBuilder(args);

// Configure your database context
builder.Services.AddDbContext<YourDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Create seeding configuration
var seedingSettings = new SeederConfiguration
{
    Environment = "Development",
    Profiles = new Dictionary<string, SeedingProfile>
    {
        ["Development"] = new()
        {
            EnabledSeeders = ["products", "users", "categories"],
            StrictMode = false
        },
        ["Production"] = new()
        {
            EnabledSeeders = ["categories"],
            StrictMode = true
        }
    },
    BatchSettings = new SeederBatchSettings
    {
        DefaultBatchSize = 100,
        SeederBatchSizes = new Dictionary<string, int>
        {
            ["users"] = 50,
            ["products"] = 25
        }
    }
};

// Register the seeder services with configuration
builder.Services.AddInzSeeder(seedingSettings)
    .UseDbContext<YourDbContext>()
    .RegisterEntitySeedersFromAssemblies(typeof(Program).Assembly)
    .RegisterEmbeddedSeedDataFromAssemblies(typeof(Program).Assembly);

var host = builder.Build();
```

### 7. Run the Seeder

Execute the seeding process using the convenient extension method:

```csharp
await app.Services.RunInzSeeder(CancellationToken.None);
```
Or
```csharp
await host.Services.RunInzSeeder(CancellationToken.None);
```

### 8. Environment-Specific Seeding

InzSeeder supports environment-specific configurations and seed data:

```csharp
// Different seed data for different environments
// SeedData/products.Development.json
// SeedData/products.Production.json
// SeedData/products.Test.json

var seedingSettings = new SeederConfiguration
{
    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
    Profiles = new Dictionary<string, SeedingProfile>
    {
        ["Development"] = new()
        {
            EnabledSeeders = ["products", "users", "categories"],
            StrictMode = false
        },
        ["Test"] = new()
        {
            EnabledSeeders = ["users"],
            StrictMode = true
        },
        ["Production"] = new()
        {
            EnabledSeeders = ["categories"],
            StrictMode = true
        }
    }
};
```

### 9. Advanced Configuration

You can also configure batch processing and other advanced features:

```csharp
var seedingSettings = new SeederConfiguration
{
    Environment = "Development",
    Profiles = new Dictionary<string, SeedingProfile>
    {
        ["Development"] = new()
        {
            EnabledSeeders = ["products", "users"],
            StrictMode = false
        }
    },
    BatchSettings = new SeederBatchSettings
    {
        DefaultBatchSize = 50, // Default batch size for all seeders
        SeederBatchSizes = new Dictionary<string, int>
        {
            ["users"] = 25,     // Custom batch size for users seeder
            ["products"] = 10   // Custom batch size for products seeder
        }
    }
};
```

## Running the Sample Projects

### InMemory Sample
```bash
cd InzSeeder.Samples.InMemory
dotnet run
```

### SQLite Sample
```bash
cd InzSeeder.Samples.SQLite
dotnet run
```

## Building and Testing

To build the solution:
```bash
dotnet build
```

To run tests:
```bash
dotnet test
```

To package as NuGet:
```bash
dotnet pack
```

## Best Practices

1. **Use Business Keys**: Always implement proper business key identification in your seeders. They must be unique in value.
2. **Environment Awareness**: Use environment-specific configurations for different deployment scenarios if necessary.
3. **Batch Processing**: Configure appropriate batch sizes for large datasets if necessary.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](InzSeeder.Core/LICENSE) file for details.