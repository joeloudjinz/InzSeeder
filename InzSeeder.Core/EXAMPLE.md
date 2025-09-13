# Inz.Seeder Usage Example

This is a simple example demonstrating how to use the Inz.Seeder package in a new project.

## Project Structure

```
ExampleProject/
├── ExampleProject.csproj
├── Program.cs
├── appsettings.json
└── SeedData/
    └── products.json
```

## ExampleProject.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">

    <PropertyGroup>
        <OutputType>Exe</OutputType>
        <TargetFramework>net9.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.8" />
        <PackageReference Include="Inz.Seeder" Version="1.0.0" />
    </ItemGroup>

</Project>
```

## Program.cs

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Inz.Seeder.Extensions;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register your DbContext
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection")));
                
                // Register the seeder
                services.AddSeeder();
            })
            .Build();

        using var scope = host.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<ISeedingOrchestrator>();
        await seeder.SeedDataAsync(CancellationToken.None);
    }
}
```

## appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ExampleDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Seeding": {
    "Profiles": {
      "Development": {
        "EnabledSeeders": ["products"],
        "StrictMode": false
      }
    }
  }
}
```

## SeedData/products.json

```json
[
  {
    "id": 1,
    "name": "Product 1",
    "price": 19.99
  },
  {
    "id": 2,
    "name": "Product 2",
    "price": 29.99
  }
]
```

This example shows how to integrate the Inz.Seeder package into a new project with minimal configuration.