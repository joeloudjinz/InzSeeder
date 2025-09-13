# Sample Project Using Inz.Seeder

This is a simple example project demonstrating how to use the Inz.Seeder package in a .NET application.

## Project Structure

- `Models/` - Contains entity models and seeder models
- `Data/` - Contains the DbContext
- `Seeders/` - Contains custom seeders
- `SeedData/` - Contains JSON seed data files

## How to Run

1. Navigate to the SampleProject directory:
   ```bash
   cd Examples/SampleProject
   ```

2. Run the project:
   ```bash
   dotnet run
   ```

This will seed the in-memory database with the sample product data.

## Customization

To use this example with your own database:

1. Update the DbContext connection string in `Program.cs`
2. Modify the entity models in `Models/`
3. Create your own seeder models
4. Create custom seeders by inheriting from `BaseEntitySeeder`
5. Add JSON seed data files to the `SeedData/` directory