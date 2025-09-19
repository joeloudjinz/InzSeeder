using InzSeeder.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace InzSeeder.Core.Extensions;

/// <summary>
/// Extension methods for configuring InzSeeder entities in a DbContext.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Configures the SeedHistory entity in the Entity Framework model.
    /// This method should be called in your DbContext's OnModelCreating method to ensure
    /// that the SeedHistory table is properly configured in the database schema.
    /// The SeedHistory entity is used by InzSeeder to track which seeders have been executed
    /// and when, enabling idempotent seeding operations.
    /// </summary>
    /// <param name="modelBuilder">The Entity Framework model builder used to configure entities.</param>
    /// <exception cref="ArgumentNullException">Thrown when modelBuilder is null.</exception>
    /// <example>
    /// <code>
    /// public class MyDbContext : DbContext
    /// {
    ///     protected override void OnModelCreating(ModelBuilder modelBuilder)
    ///     {
    ///         // Configure InzSeeder entities
    ///         modelBuilder.AddInzSeederEntities();
    ///         
    ///         // Other entity configurations...
    ///     }
    /// }
    /// </code>
    /// </example>
    public static void AddInzSeederEntities(this ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        
        modelBuilder.Entity<SeedHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SeedIdentifier).IsUnique();
            entity.Property(e => e.SeedIdentifier).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ContentHash).IsRequired().HasMaxLength(64);
        });
    }
}