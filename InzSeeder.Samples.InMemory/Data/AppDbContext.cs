using InzSeeder.Samples.InMemory.Models;
using Microsoft.EntityFrameworkCore;

namespace InzSeeder.Samples.InMemory.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;
}