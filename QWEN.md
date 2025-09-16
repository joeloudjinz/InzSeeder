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
- `Abstractions/` - Base classes like `BaseEntitySeeder`
- `Contracts/` - Interfaces defining the core contracts

### InzSeeder.Samples.InMemory
A sample project demonstrating how to use the InzSeeder library.

Key files and directories:
- `Program.cs` - Sample application entry point
- `InzSeeder.Samples.InMemory.csproj` - Project configuration
- `Models/` - Entity and seeder models
- `Data/` - DbContext implementation
- `Seeders/` - Custom seeder implementations
- `SeedData/` - JSON seed data files

## Technology Stack

- **Language**: C# 13
- **Framework**: .NET Core (Version 9)
- **ORM**: Entity Framework Core
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Configuration**: Microsoft.Extensions.Configuration
- **Logging**: Microsoft.Extensions.Logging

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

### Sample Project
To run the sample project:
```bash
cd InzSeeder.Samples.InMemory
dotnet run
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