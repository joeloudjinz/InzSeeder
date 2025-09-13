using System.Text.Json;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Abstractions;

/// <summary>
/// Abstract base class for entity seeders that implements the template method pattern.
/// </summary>
/// <typeparam name="TEntity">The type of entity to seed.</typeparam>
/// <typeparam name="TModel">The type of model used for seeding.</typeparam>
public abstract class BaseEntitySeeder<TEntity, TModel> : IEntitySeeder
    where TEntity : class
    where TModel : class
{
    private readonly ISeedDataProvider _seedDataProvider;
    private readonly ISeederDbContext _dbContext;
    private readonly ILogger<BaseEntitySeeder<TEntity, TModel>> _logger;
    private readonly SeedingSettings? _seedingSettings;
    private readonly SeedingPerformanceMetricsService? _performanceMetricsService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseEntitySeeder{TEntity, TModel}"/> class.
    /// </summary>
    /// <param name="seedDataProvider">The seed data provider.</param>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="seedingSettings">The seeding settings (optional).</param>
    /// <param name="performanceMetricsService">The performance metrics service (optional).</param>
    protected BaseEntitySeeder(
        ISeedDataProvider seedDataProvider,
        ISeederDbContext dbContext,
        ILogger<BaseEntitySeeder<TEntity, TModel>> logger,
        SeedingSettings? seedingSettings = null,
        SeedingPerformanceMetricsService? performanceMetricsService = null
    )
    {
        _seedDataProvider = seedDataProvider;
        _dbContext = dbContext;
        _logger = logger;
        _seedingSettings = seedingSettings;
        _performanceMetricsService = performanceMetricsService;
    }

    /// <inheritdoc/>
    public abstract string SeedName { get; }

    /// <inheritdoc/>
    public virtual IEnumerable<Type> Dependencies => [];

    /// <inheritdoc/>
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting seeder '{SeederName}'", SeedName);

        // Start performance measurement
        using var metricsToken = _performanceMetricsService?.StartMeasurement(SeedName);

        // 1. Calling the data provider to get content and hash
        var (content, hash) = await _seedDataProvider.GetSeedDataAsync(SeedName, cancellationToken);
        if (content == null || hash == null)
        {
            _logger.LogWarning("No seed data found for seeder '{SeederName}'", SeedName);
            return;
        }

        // 2. Querying the SeedHistory table to check if the hash matches
        var existingSeedHistory = await _dbContext.Set<SeedHistory>().FirstOrDefaultAsync(sh => sh.SeedIdentifier == SeedName, cancellationToken);
        if (existingSeedHistory != null && existingSeedHistory.ContentHash == hash)
        {
            _logger.LogWarning("Seeder '{SeederName}' has already been applied with the same content. Skipping.", SeedName);
            return;
        }

        // 3. Deserializing the JSON into a List<TModel>
        List<TModel>? models;
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            models = JsonSerializer.Deserialize<List<TModel>>(content, options);
        }
        catch (JsonException ex)
        {
            _logger.LogCritical(ex, "Failed to deserialize seed data for seeder '{SeederName}'. Invalid JSON format.", SeedName);
            return;
        }

        if (models == null || models.Count == 0)
        {
            _logger.LogError("No data to seed for seeder '{SeederName}'", SeedName);
            return;
        }

        // Get batch size for this seeder
        var batchSize = GetBatchSize();

        _logger.LogInformation("Processing {ModelCount} models with batch size {BatchSize}", models.Count, batchSize);

        // 4. Fetching all existing entities into an in-memory dictionary, keyed by their business key
        var existingEntities = await _dbContext.Set<TEntity>().ToListAsync(cancellationToken);
        var existingEntitiesDict = existingEntities.ToDictionary(GetBusinessKeyFromEntity, e => e);

        // 5. Process models in batches
        var processedCount = 0;
        for (var i = 0; i < models.Count; i += batchSize)
        {
            var batch = models.Skip(i).Take(batchSize).ToList();
            _logger.LogInformation("Processing batch {BatchNumber} with {BatchSize} items", i / batchSize + 1, batch.Count);

            // Process each model in the batch
            foreach (var model in batch)
            {
                var businessKey = GetBusinessKey(model);
                if (existingEntitiesDict.TryGetValue(businessKey, out var existingEntity))
                {
                    // Update existing entity
                    UpdateEntity(existingEntity, model);
                }
                else
                {
                    // Create new entity
                    var newEntity = MapToEntity(model);

                    // Set IsSystemOwned flag for new entities that support it
                    if (newEntity is ISystemOwnedEntity systemOwnedEntity) systemOwnedEntity.IsSystemOwned = true;
                    await _dbContext.Set<TEntity>().AddAsync(newEntity, cancellationToken);
                }

                processedCount++;
            }

            // Save changes after each batch
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Completed batch {BatchNumber}, processed {ProcessedCount}/{TotalCount} items", i / batchSize + 1, processedCount, models.Count);
        }

        // 6. Updating the SeedHistory table upon success
        if (existingSeedHistory != null)
        {
            existingSeedHistory.ContentHash = hash;
            existingSeedHistory.AppliedDateUtc = DateTime.UtcNow;
        }
        else
        {
            var newSeedHistory = new SeedHistory
            {
                SeedIdentifier = SeedName,
                ContentHash = hash,
                AppliedDateUtc = DateTime.UtcNow
            };
            await _dbContext.Set<SeedHistory>().AddAsync(newSeedHistory, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeder '{SeederName}' completed successfully. Processed {ProcessedCount} items", SeedName, processedCount);
    }

    /// <summary>
    /// Gets the business key from an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The business key.</returns>
    protected abstract object GetBusinessKeyFromEntity(TEntity entity);

    /// <summary>
    /// Gets the business key from a model.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The business key.</returns>
    protected abstract object GetBusinessKey(TModel model);

    /// <summary>
    /// Maps a model to an entity.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The entity.</returns>
    protected abstract TEntity MapToEntity(TModel model);

    /// <summary>
    /// Updates an existing entity with data from a model.
    /// </summary>
    /// <param name="existingEntity">The existing entity.</param>
    /// <param name="model">The model.</param>
    protected abstract void UpdateEntity(TEntity existingEntity, TModel model);

    /// <summary>
    /// Gets the batch size for this seeder.
    /// </summary>
    /// <returns>The batch size.</returns>
    private int GetBatchSize()
    {
        // If no settings provided, use default batch size
        if (_seedingSettings?.BatchSettings == null) return 100;

        // Check if there's a specific batch size for this seeder, otherwise, use the default batch size from settings
        return _seedingSettings.BatchSettings.SeederBatchSizes.TryGetValue(SeedName, out var batchSize) ? batchSize : _seedingSettings.BatchSettings.DefaultBatchSize;
    }
}