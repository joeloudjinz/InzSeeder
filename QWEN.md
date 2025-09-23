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
- **Hash-Based Change Detection**: Uses content hashes to determine if seed data has changed
- **Entity Reference Resolution**: Resolve references between entities created during seeding
- **Extensible Architecture**: Easy to create custom seeders

## Project Structure

The solution contains the following projects:

1. **InzSeeder.Core** - The main library implementation
2. **InzSeeder.Core.Tests** - Unit and integration tests for the core library
3. **InzSeeder.Samples.InMemory** - A sample project demonstrating usage with an in-memory database
4. **InzSeeder.Samples.Web** - A sample web project demonstrating usage with a SQLite database

### InzSeeder.Core
This is the main library project that implements the data seeding functionality.

Key files and directories:
- `InzSeeder.Core.csproj` - Project configuration with NuGet package settings
- `Contracts/` - Interfaces defining the core contracts (IEntityDataSeeder, ISeedDataProvider, etc.)
- `Extensions/` - Extension methods for service registration and execution
- `Models/` - Configuration models (SeederConfiguration, SeedingProfile, etc.)
- `Services/` - Core services (EmbeddedResourceSeedDataProvider, EntityReferenceResolver, etc.)
- `Algorithms/` - Core algorithms (EnvironmentSeedingOrchestrator, SeederExecutor, etc.)
- `Attributes/` - Custom attributes (EnvironmentCompatibilityAttribute)
- `Utilities/` - Utility classes (EnvironmentUtility)
- `Builder/` - Fluent builder pattern implementation (SeederBuilder)

### InzSeeder.Core.Tests
Unit and integration tests for the core library.

Key files and directories:
- `InzSeeder.Core.Tests.csproj` - Test project configuration
- `Entities/` - Test entity models
- `Seeders/` - Test seeder implementations
- `Data/` - Test DbContext implementations
- `SeedData/` - Test seed data files
- `IntegrationTests/` - Integration test suites

### InzSeeder.Samples.InMemory
A sample project demonstrating how to use the InzSeeder library with an in-memory database.

Key files and directories:
- `Program.cs` - Sample application entry point
- `InzSeeder.Samples.InMemory.csproj` - Project configuration
- `Models/` - Entity and seeder models
- `Data/` - DbContext implementation
- `Seeders/` - Custom seeder implementations
- `SeedData/` - JSON seed data files

### InzSeeder.Samples.Web
A sample web project demonstrating how to use the InzSeeder library with a SQLite database in a web application.

Key files and directories:
- `Program.cs` - Sample web application entry point
- `InzSeeder.Samples.Web.csproj` - Project configuration
- `Models/` - Entity and seeder models, including examples of entity reference resolution
- `Data/` - DbContext implementation
- `Seeders/` - Custom seeder implementations demonstrating entity reference resolution
- `SeedData/` - JSON seed data files with environment-specific variations and entity references
- `Migrations/` - EF Core migrations
- `appsettings*.json` - Configuration files

## Technology Stack

- **Language**: C# 13
- **Framework**: .NET Core (Version 9)
- **ORM**: Entity Framework Core
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Configuration**: Microsoft.Extensions.Configuration
- **Logging**: Microsoft.Extensions.Logging
- **Testing**: xUnit, Microsoft.NET.Test.Sdk
- **Code Coverage**: coverlet.collector

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
cd InzSeeder.Core
dotnet pack
```

### Sample Projects
To run the InMemory sample project:
```bash
cd InzSeeder.Samples.InMemory
dotnet run
```

To run the Web sample project in seed mode:
```bash
cd InzSeeder.Samples.Web
dotnet run seedMode
```

## Development Conventions

1. **Generic Design**: All components are designed to work with any Entity Framework Core DbContext
2. **Idempotency**: Seeders can be run multiple times without creating duplicates
3. **Dependency Injection**: Heavy use of Microsoft's DI container for service resolution
4. **Configuration-Driven**: Behavior is controlled through programmatic configuration
5. **Logging**: Comprehensive logging using Microsoft's logging framework
6. **Error Handling**: Graceful error handling with meaningful error messages
7. **Performance**: Batch processing for large datasets to optimize performance
8. **Environment Awareness**: Support for environment-specific seeding configurations
9. **Entity Reference Resolution**: Use string-based keys to reference entities created during seeding

## Key Implementation Details

1. **Hash-Based Change Detection**: Uses content hashes to determine if seed data has changed
2. **Batch Processing**: Processes entities in configurable batch sizes to handle large datasets efficiently
3. **Business Key Pattern**: Uses business keys (rather than primary keys) to identify existing entities
4. **Seeder Dependencies**: Supports dependency management between seeders through the Dependencies property
5. **Environment-Specific Seeding**: Allows different seeding behavior based on environment
6. **Seeder Profiles**: Supports different seeder configurations for different environments
7. **Strict Mode**: Configuration option to only run explicitly enabled seeders
8. **Environment Compatibility**: Attribute-based and interface-based environment restrictions
9. **Entity Reference Resolution**: Allows seeders to reference entities created during seeding using string-based keys

## Extensibility Points

1. **Custom Seeders**: Implement `IEntityDataSeeder<TEntity, TModel>` to create custom seeders
2. **Environment-Aware Seeders**: Implement `IEnvironmentAwareSeeder` or use `EnvironmentCompatibilityAttribute`
3. **Custom Data Providers**: Implement `ISeedDataProvider` to load data from different sources
4. **Custom DbContext**: Implement `ISeederDbContext` to work with custom database contexts
5. **Seeder Dependencies**: Specify dependencies between seeders using the Dependencies property
6. **Entity Reference Resolution**: Use `IEntityReferenceResolver` to resolve references between entities created during seeding