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
The main library implementation. See [InzSeeder.Core/README.md](InzSeeder.Core/README.md) for detailed documentation.

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

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](InzSeeder.Core/LICENSE) file for details.