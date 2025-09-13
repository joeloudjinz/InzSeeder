# Inz.Seeder

Inz.Seeder is a flexible, generic data seeding library for .NET applications that can be used to seed any database with initial data.

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

Add the Inz.Seeder NuGet package to your project:

```bash
dotnet add package Inz.Seeder
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

2. Register the seeder in your application:

```csharp
services.AddSeeder<YourDbContext>((options) =>
{
    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
});
```

3. Add JSON seed data to the `SeedData` folder:

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
dotnet run --project Inz.Seeder
```

## Documentation

See the [README.md](README.md) file for detailed documentation on configuration, environment-specific seeding, and advanced features.

## Examples

See the [Examples/SampleProject](Examples/SampleProject/README.md) directory for a complete example of how to use the Inz.Seeder package in a .NET application.

## Testing

Run the tests with:

```bash
dotnet test
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.