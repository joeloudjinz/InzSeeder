using InzSeeder.Core.Contracts;
using InzSeeder.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace InzSeeder.Core.StressTest.Data;

/// <summary>
/// Database context for stress testing the seeding process.
/// </summary>
public class StressTestDbContext : DbContext, ISeederDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StressTestDbContext"/> class.
    /// </summary>
    /// <param name="options">The options for this context.</param>
    public StressTestDbContext(DbContextOptions<StressTestDbContext> options) : base(options)
    {
    }
    
    /// <summary>
    /// Gets or sets the test entities.
    /// </summary>
    public DbSet<TestEntity> TestEntities { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the related test entities.
    /// </summary>
    public DbSet<RelatedTestEntity> RelatedTestEntities { get; set; } = null!;
    
    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.BusinessKey).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });
        
        modelBuilder.Entity<RelatedTestEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.BusinessKey).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(255);
        });
        
        // Add InzSeeder entities
        modelBuilder.AddInzSeederEntities();
        
        base.OnModelCreating(modelBuilder);
    }
}