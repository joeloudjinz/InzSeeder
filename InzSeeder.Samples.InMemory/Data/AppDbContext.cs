using InzSeeder.Core.Extensions;
using InzSeeder.Samples.InMemory.Models;
using Microsoft.EntityFrameworkCore;

namespace InzSeeder.Samples.InMemory.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the SeedHistory entity for InzSeeder
        modelBuilder.AddInzSeederEntities();
        
        base.OnModelCreating(modelBuilder);
    }
}