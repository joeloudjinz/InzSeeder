using InzSeeder.Core.Contracts;
using InzSeeder.Core.Utilities;
using Microsoft.Extensions.Logging;
using static System.Console;

namespace InzSeeder.Core.Services;

/// <summary>
/// Service for purging all existing records from the database.
/// 
/// The purge operation is NOT ALLOWED in Production environment.
/// Allowed environments: Development, Staging
/// </summary>
public class SeederPurgeService
{
    private readonly ISeederDbContext _dbContext;
    private readonly ILogger<SeederPurgeService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeederPurgeService"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger.</param>
    public SeederPurgeService(ISeederDbContext dbContext, ILogger<SeederPurgeService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Purges all existing records from the database with proper environment checks and user confirmation.
    /// </summary>
    /// <param name="bypassConfirmation">Whether to bypass user confirmation.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if purge was successful, false otherwise.</returns>
    public async Task<bool> PurgeAsync(bool bypassConfirmation, CancellationToken cancellationToken)
    {
        var environment = EnvironmentUtility.Environment();
        
        // Check if we're in a production environment
        if (string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("Purge operation is not allowed in Production environment");
            await Error.WriteLineAsync("ERROR: Purge operation is not allowed in Production environment");
            return false;
        }

        // Get confirmation from user if not bypassed
        if (!bypassConfirmation)
        {
            WriteLine($"WARNING: You are about to purge ALL data from the database in {environment} environment.");
            WriteLine("This operation cannot be undone.");
            Write("Are you sure you want to continue? (type 'yes' to confirm): ");
            
            var confirmation = ReadLine();
            if (!string.Equals(confirmation, "yes", StringComparison.OrdinalIgnoreCase))
            {
                WriteLine("Purge operation cancelled.");
                return false;
            }
        }
        else
        {
            WriteLine($"Purging ALL data from the database in {environment} environment (confirmation bypassed)...");
        }

        try
        {
            // Start a transaction for the purge operation
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            
            _logger.LogInformation("Starting database purge operation in {Environment} environment", environment);
            
            // TODO: Delete all records from all tables
                        
            // Commit the transaction
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Database purge operation completed successfully");
            WriteLine("Database purge completed successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database purge operation failed");
            await Error.WriteLineAsync($"ERROR: Database purge operation failed: {ex.Message}");
            return false;
        }
    }
}