using InzSeeder.Core.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace InzSeeder.Core.Adapters;

/// <summary>
/// Adapter that wraps a SeederDbContext to implement ISeederDbContext.
/// </summary>
/// <typeparam name="TUserContext">The user's DbContext type.</typeparam>
internal class SeederDbContextAdapter<TUserContext> : ISeederDbContext where TUserContext : DbContext
{
    private readonly TUserContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeederDbContextAdapter{TUserContext}"/> class.
    /// </summary>
    /// <param name="context">The SeederDbContext instance.</param>
    public SeederDbContextAdapter(TUserContext context)
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