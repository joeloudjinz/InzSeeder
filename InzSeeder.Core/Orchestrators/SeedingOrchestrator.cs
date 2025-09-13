using InzSeeder.Core.Contracts;
using InzSeeder.Core.Services;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Orchestrators;

/// <summary>
/// Orchestrates the seeding process.
/// </summary>
public class SeedingOrchestrator : ISeedingOrchestrator
{
    private readonly IEnumerable<IEntitySeeder> _seeders;
    private readonly ISeederDbContext _dbContext;
    private readonly ILogger<SeedingOrchestrator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedingOrchestrator"/> class.
    /// </summary>
    /// <param name="seeders">The collection of entity seeders.</param>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger.</param>
    public SeedingOrchestrator(
        IEnumerable<IEntitySeeder> seeders,
        ISeederDbContext dbContext,
        ILogger<SeedingOrchestrator> logger
    )
    {
        _seeders = seeders;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting environment-independent data seeding process");

        // Sort seeders based on their dependencies
        var sortedSeeders = SeederSorter.Sort(_seeders).ToList();
        _logger.LogInformation("Found {SeederCount} seeders to execute", sortedSeeders.Count);

        // Create a database transaction
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Execute each seeder in the sorted order
            foreach (var seeder in sortedSeeders)
            {
                _logger.LogInformation("Executing seeder '{SeederName}'", seeder.SeedName);
                await seeder.ExecuteAsync(cancellationToken);
            }

            // Commit the transaction
            await transaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Data seeding process completed successfully");
        }
        catch (Exception ex)
        {
            // Rollback the transaction on any exception
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Data seeding process failed");
            throw;
        }
    }
}