using InzSeeder.Core.Extensions;
using InzSeeder.Samples.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace InzSeeder.Samples.Web.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<NotificationType> NotificationTypes => Set<NotificationType>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure the SeedHistory entity for InzSeeder
        modelBuilder.AddInzSeederEntities();

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(50);
        });

        modelBuilder.Entity<NotificationType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TargetAudience).IsRequired().HasMaxLength(50);
            
            entity.HasOne(e => e.LastModifiedByUser)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.NotificationTemplate)
                .WithOne(e => e.NotificationType)
                .HasForeignKey<NotificationType>(e => e.NotificationTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DeliveryMethod).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Body).IsRequired();

            entity.HasOne(e => e.LastModifiedByUser)
                .WithMany()
                .HasForeignKey(e => e.LastModifiedByUserId)
                .OnDelete(DeleteBehavior.Cascade);

            // entity.HasOne(e => e.NotificationType)
            //     .WithOne(e => e.NotificationTemplate)
            //     .HasForeignKey<NotificationTemplate>(e => e.NotificationTypeId)
            //     .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}