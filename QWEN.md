# InzSeeder Project Context

## Project Overview

InzSeeder is a flexible, generic data seeding library for .NET applications that can be used to seed any database with initial data. It provides a robust, idempotent, and configurable way to populate databases with seed data.

### Key Features
- **Generic Design**: Works with any Entity Framework Core DbContext
- **Idempotent Seeding**: Safe to run multiple times without creating duplicates
- **Environment-Aware**: Supports environment-specific seeding configurations
- **Dependency Management**: Handles dependencies between seeders
- **Batch Processing**: Processes large datasets in batches for better performance
- **Audit Logging**: Tracks all seeding operations
- **Hash-Based Change Detection**: Only re-seeds when data changes
- **Extensible Architecture**: Easy to create custom seeders
- **Command-Line Interface**: Rich CLI with preview, dry-run, and health check modes

## Project Structure

The solution contains two main projects:

1. **InzSeeder.Core** - The main library implementation
2. **InzSeeder.Samples.InMemory** - A sample project demonstrating usage with an in-memory database

### InzSeeder.Core
This is the main library project that implements the data seeding functionality.

Key files and directories:
- `Program.cs` - Entry point with CLI argument parsing
- `InzSeeder.Core.csproj` - Project configuration with NuGet package settings
- `README.md` - Documentation and usage instructions
- `Abstractions/` - Base classes like `BaseEntitySeeder`
- `Contracts/` - Interfaces defining the core contracts
- `SeedData/` - Directory for JSON seed data files (embedded as resources)

### InzSeeder.Samples.InMemory
A sample project demonstrating how to use the InzSeeder library.

Key files and directories:
- `Program.cs` - Sample application entry point
- `InzSeeder.Samples.InMemory.csproj` - Project configuration
- `README.md` - Sample project documentation
- `Models/` - Entity and seeder models
- `Data/` - DbContext implementation
- `Seeders/` - Custom seeder implementations
- `SeedData/` - JSON seed data files

## Technology Stack

- **Language**: C# (.NET 9.0)
- **Framework**: .NET Core
- **ORM**: Entity Framework Core
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Configuration**: Microsoft.Extensions.Configuration
- **Logging**: Microsoft.Extensions.Logging
- **CLI Parsing**: Custom command-line argument parser

## Core Concepts

### BaseEntitySeeder
The `BaseEntitySeeder<TEntity, TModel>` is an abstract base class that implements the template method pattern for entity seeders. It provides a complete implementation for:
1. Loading seed data from JSON files
2. Checking if data has already been seeded using hash-based change detection
3. Deserializing JSON into model objects
4. Fetching existing entities from the database
5. Processing entities in batches
6. Updating existing entities or creating new ones
7. Recording seeding history

### Interfaces
- `IEntitySeeder` - Core interface for all seeders
- `ISeedingOrchestrator` - Manages the overall seeding process
- `ISeedDataProvider` - Provides seed data from various sources
- `ISeederDbContext` - Database context interface

## Usage Patterns

### Creating a Custom Seeder
To create a custom seeder, inherit from `BaseEntitySeeder` and implement the abstract methods:

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

### Registration
Register the seeder in your application with configuration using the unified fluent API:

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

### Configuration
The library now accepts configuration externally rather than requiring appsettings.json files. You can configure the seeder programmatically when calling `AddInzSeeder()`:

```csharp
var seedingSettings = new SeedingSettings
{
    Environment = "Development",
    Profiles = new Dictionary<string, SeedingProfile>
    {
        ["Development"] = new SeedingProfile
        {
            EnabledSeeders = ["roles", "users"],
            StrictMode = false
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

## Building and Running

### Library Development
To build the library:
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

### Sample Project
To run the sample project:
```bash
cd InzSeeder.Samples.InMemory
dotnet run
```

### CLI Usage
The library provides a rich CLI with several options:
```bash
dotnet run --project InzSeeder.Core
dotnet run --project InzSeeder.Core -- --help
dotnet run --project InzSeeder.Core -- --environment Production
dotnet run --project InzSeeder.Core -- --preview
dotnet run --project InzSeeder.Core -- --health-check
```

## Development Conventions

1. **Generic Design**: All components are designed to work with any Entity Framework Core DbContext
2. **Idempotency**: Seeders can be run multiple times without creating duplicates
3. **Dependency Injection**: Heavy use of Microsoft's DI container for service resolution
4. **Configuration-Driven**: Behavior is controlled through programmatic configuration
5. **Logging**: Comprehensive logging using Microsoft's logging framework
6. **Error Handling**: Graceful error handling with meaningful error messages
7. **Performance**: Batch processing for large datasets to optimize performance

## Key Implementation Details

1. **Hash-Based Change Detection**: Uses content hashes to determine if seed data has changed
2. **Batch Processing**: Processes entities in configurable batch sizes to handle large datasets efficiently
3. **Business Key Pattern**: Uses business keys (rather than primary keys) to identify existing entities
4. **Seeder Dependencies**: Supports dependency management between seeders
5. **Environment-Specific Seeding**: Allows different seeding behavior based on environment
6. **Seeder Profiles**: Supports different seeder configurations for different environments

## Extensibility Points

1. **Custom Seeders**: Inherit from `BaseEntitySeeder` to create custom seeders
2. **Custom Data Providers**: Implement `ISeedDataProvider` to load data from different sources
3. **Custom DbContext**: Implement `ISeederDbContext` to work with custom database contexts
4. **Custom Seeding Orchestrator**: Implement `ISeedingOrchestrator` to customize the seeding process flow