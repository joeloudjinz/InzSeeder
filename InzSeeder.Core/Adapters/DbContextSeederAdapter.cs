using InzSeeder.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace InzSeeder.Core.Adapters;

/// <summary>
/// Adapter that wraps an existing DbContext to implement ISeederDbContext.
/// </summary>
/// <typeparam name="TContext">The type of the existing DbContext.</typeparam>
internal class DbContextSeederAdapter<TContext> : ISeederDbContext where TContext : DbContext
{
    private readonly TContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextSeederAdapter{TContext}"/> class.
    /// </summary>
    /// <param name="context">The existing DbContext instance.</param>
    public DbContextSeederAdapter(TContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public DatabaseFacade Database => _context.Database;

    /// <inheritdoc/>
    public ChangeTracker ChangeTracker => _context.ChangeTracker;

    /// <inheritdoc/>
    public DbSet<TEntity> Set<TEntity>() where TEntity : class
    {
        return _context.Set<TEntity>();
    }

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}