# InzSeeder

InzSeeder is a flexible, generic data seeding library for .NET applications that can be used to seed any database with initial data.

[![Full Unit Tests](https://github.com/joeloudjinz/InzSeeder/actions/workflows/run-tests.yml/badge.svg)](https://github.com/joeloudjinz/InzSeeder/actions/workflows/run-tests.yml)

## Features

- **Generic Design**: Works with any Entity Framework Core DbContext
- **Idempotent Seeding**: Safe to run multiple times without creating duplicates
- **Environment-Aware**: Supports environment-specific seeding configurations
- **Dependency Management**: Handles dependencies between seeders
- **Batch Processing**: Processes large datasets in batches for better performance
- **Hash-Based Change Detection**: Only re-seeds when data changes
- **Entity Reference Resolution**: Resolve references between entities created during seeding
- **Extensible Architecture**: Easy to create custom seeders

## Projects

This solution contains the following projects:

### InzSeeder.Core
The main library implementation.

### InzSeeder.Core.StressTest
A comprehensive stress testing tool for the InzSeeder library that provides detailed performance metrics and analysis.

### InzSeeder.Samples.InMemory
A sample project demonstrating usage with an in-memory database.

### InzSeeder.Samples.Web
A sample project demonstrating usage with a SQLite database in a web application.

## Quick Start

To get started with InzSeeder, you can either:

1. Add the NuGet package to your project:
   ```bash
   dotnet add package InzSoftwares.NetSeeder
   ```

2. Or clone this repository and explore the sample projects:
   ```bash
   git clone https://github.com/joeloudjinz/InzSeeder.git
   cd InzSeeder
   ```

## Detailed Usage Guide

### 1. Installation

Add the InzSeeder NuGet package to your project:

```bash
dotnet add package InzSoftwares.NetSeeder
```

### 2. Create Your Data Models

First, create your entity models that represent the data you want to seed:

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
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
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

### 4. Create Custom Seeders

Create custom seeders by implementing `IEntityDataSeeder<TEntity, TModel>`. The new entity reference resolution feature allows you to reference entities created during seeding using string keys:

```csharp
public class ProductSeeder : IEntityDataSeeder<Product, ProductSeedModel>
{
    public string SeedName => "products";

    public IEnumerable<Type> Dependencies { get; } = [];

    // You can use other properties of different type based on your choice, like `string Key`
    public object GetBusinessKeyFromEntity(Product entity) => entity.Id;

    // You can use other properties of different type based on your choice, like `string Key`
    public object GetBusinessKey(ProductSeedModel model) => model.Id;

    public Product MapEntity(ProductSeedModel model, IEntityReferenceResolver referenceResolver)
    {
        return new Product
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateEntity(Product existingEntity, ProductSeedModel model, IEntityReferenceResolver referenceResolver)
    {
        existingEntity.Name = model.Name;
        existingEntity.Description = model.Description;
        existingEntity.Price = model.Price;
        existingEntity.UpdatedAt = DateTime.UtcNow;
    }
}
```

### 5. Entity Reference Resolution

InzSeeder now supports entity reference resolution, allowing you to reference entities created during seeding by using string-based keys instead of hardcoded IDs. This is especially useful when you have relationships between entities that are being seeded.

To use this feature:

1. Make your seed models implement `IHasKeyModel` to provide a string key:

```csharp
public class ProductSeedModel : IHasKeyModel
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
```

2. In your seeder, use the `referenceResolver` parameter to resolve entity references:

```csharp
public class OrderSeeder : IEntityDataSeeder<Order, OrderSeedModel>
{
    public string SeedName => "orders";
    
    public IEnumerable<Type> Dependencies { get; } = [typeof(ProductSeeder)];

    public object GetBusinessKeyFromEntity(Order entity) => entity.Id;
    public object GetBusinessKey(OrderSeedModel model) => model.Id;

    public Order MapEntity(OrderSeedModel model, IEntityReferenceResolver referenceResolver)
    {
        // Resolve the Product reference by its key
        var product = referenceResolver.ResolveEntity<Product>(model.ProductKey);
        
        return new Order
        {
            Id = model.Id,
            ProductId = product.Id,
            Quantity = model.Quantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateEntity(Order existingEntity, OrderSeedModel model, IEntityReferenceResolver referenceResolver)
    {
        // Update the Product reference if needed
        var product = referenceResolver.ResolveEntity<Product>(model.ProductKey);
        existingEntity.ProductId = product.Id;
        existingEntity.Quantity = model.Quantity;
        existingEntity.UpdatedAt = DateTime.UtcNow;
    }
}
```

3. In your seed data JSON files, use the string keys to reference other entities:

```json
// SeedData/products.json
[
  {
    "key": "laptop-product",
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 999.99
  }
]

// SeedData/orders.json
[
  {
    "id": 1,
    "productKey": "laptop-product",
    "quantity": 2
  }
]
```

The entity reference resolver automatically registers entities with their keys during the seeding process, making them available for resolution by other seeders.

### 5. Prepare Seed Data

Create JSON files in a `SeedData` folder in your project:

```json
// SeedData/products.json
[
  {
    "id": 1,
    "name": "Laptop",
    "description": "High-performance laptop",
    "price": 999.99
  },
  {
    "id": 2,
    "name": "Mouse",
    "description": "Wireless mouse",
    "price": 29.99
  }
]
```

For environment-specific data, create files with environment suffixes:
```json
// SeedData/products.Development.json
// SeedData/products.Production.json
// SeedData/products.Test.json
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

In your application's startup code (Program.cs), configure the InzSeeder services:

```csharp
using InzSeeder.Core.Extensions;
using InzSeeder.Core.Models;

var builder = Host.CreateApplicationBuilder(args);

// Configure your database context
builder.Services.AddDbContext<YourDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Create seeding configuration
var seedingSettings = new SeederConfiguration
{
    Profile = new SeedingProfile
    {
        EnabledSeeders = ["products", "users", "categories"],
        StrictMode = false
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
    Profile = new SeedingProfile
    {
        EnabledSeeders = ["products", "users", "categories"],
        StrictMode = false
    },
    BatchSettings = new SeederBatchSettings
    {
        DefaultBatchSize = 100
    }
};
```

Environment is determined in this order:
1. passed argument
2. `SEEDING_ENVIRONMENT` environment variable
3. `ASPNETCORE_ENVIRONMENT` environment variable

### 9. Advanced Configuration

You can configure batch processing and other advanced features:

```csharp
var seedingSettings = new SeederConfiguration
{
    Profile = new SeedingProfile
    {
        EnabledSeeders = ["products", "users"],
        StrictMode = false
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

### 10. Environment-Aware Seeders

For more control over which seeders run in which environments, you can implement `IEnvironmentAwareSeeder`:

```csharp
public class ProductSeeder : IEntityDataSeeder<Product, ProductSeedModel>, IEnvironmentAwareSeeder
{
    // ... other implementation details
    
    public bool ShouldRunInEnvironment(string environment)
    {
        // Only run in Development and Staging environments
        return environment == "Development" || environment == "Staging";
    }
}
```

Alternatively, use the `[EnvironmentCompatibility]` attribute:

```csharp
[EnvironmentCompatibility(productionSafe: false, "Development", "Staging")]
public class ProductSeeder : IEntityDataSeeder<Product, ProductSeedModel>
{
    // ... implementation
}
```

## Running the Sample Projects

### InMemory Sample
```bash
cd InzSeeder.Samples.InMemory
dotnet run
```

### Web Sample
```bash
cd InzSeeder.Samples.Web
dotnet run seedMode # seedMode indicate that the web project should be ran in seed mode
```

### Stress Test
```bash
cd InzSeeder.Core.StressTest
dotnet run Large # You can use arguments: Small, Medium, Large, ExtraLarge
```

The stress test project provides comprehensive performance metrics for the seeding process. See [InzSeeder.Core.StressTest/README.md](InzSeeder.Core.StressTest/README.md) for detailed documentation.

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
cd InzSeeder.Core
dotnet pack
```

## Best Practices

1. **Use Business Keys**: Always implement proper business key identification in your seeders. They must be unique in value.
2. **Environment Awareness**: Use environment-specific configurations for different deployment scenarios if necessary.
3. **Batch Processing**: Configure appropriate batch sizes for large datasets if necessary.
4. **Idempotency**: Design your seeders to be idempotent so they can be safely run multiple times.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](InzSeeder.Core/LICENSE) file for details.