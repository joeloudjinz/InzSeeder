using System.Text.Json;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Algorithms;

/// <summary>
/// Executes the logic for a single entity seeder.
/// </summary>
/// <remarks>
/// This internal static class is responsible for the detailed process of seeding data for a specific entity.
/// Its tasks include:
/// - Retrieving seed data and its hash from a data provider.
/// - Checking for existing seed history to prevent redundant executions.
/// - Deserializing the seed data from JSON into models.
/// - Processing the data in batches to manage memory and performance.
/// - Identifying new versus existing entities based on a business key.
/// - Creating new entities or updating existing ones.
/// - Recording the outcome in the seed history table.
/// </remarks>
internal static class SeederExecutor
{
    /// <summary>
    /// Executes the seeding process for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be seeded.</typeparam>
    /// <typeparam name="TModel">The type of the data model used for seeding.</typeparam>
    /// <param name="seeder">The seeder instance that defines the seeding logic.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies like the DbContext and data provider.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    internal static async Task Execute<TEntity, TModel>(
        IEntityDataSeeder<TEntity, TModel> seeder,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default
    )
        where TEntity : class
        where TModel : class
    {
        var performanceMetricsService = serviceProvider.GetRequiredService<SeedingPerformanceMetricsService>();
        var logger = serviceProvider.GetRequiredService<ILogger<IEntityDataSeeder<TEntity, TModel>>>();
        var seedingSettings = serviceProvider.GetRequiredService<SeederConfiguration>();
        var seedDataProvider = serviceProvider.GetRequiredService<ISeedDataProvider>();
        var dbContext = serviceProvider.GetRequiredService<ISeederDbContext>();
        var referenceResolver = serviceProvider.GetRequiredService<IEntityReferenceResolver>();

        var seedName = seeder.SeedName;
        logger.LogInformation("Starting seeder '{SeederName}'", seedName);

        using var metricsToken = performanceMetricsService.StartMeasurement(seedName);

        var (content, hash) = await seedDataProvider.GetSeedDataAsync(seedName, cancellationToken);
        if (content == null || hash == null)
        {
            logger.LogWarning("No seed data found for seeder '{SeederName}'", seedName);
            return;
        }

        var existingSeedHistory = await dbContext.Set<SeedHistory>().FirstOrDefaultAsync(sh => sh.SeedIdentifier == seedName, cancellationToken);
        if (existingSeedHistory != null && existingSeedHistory.ContentHash == hash)
        {
            logger.LogWarning("Seeder '{SeederName}' has already been applied with the same content. Skipping.", seedName);
            return;
        }

        List<TModel>? models;
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            models = JsonSerializer.Deserialize<List<TModel>>(content, options);
        }
        catch (JsonException ex)
        {
            logger.LogCritical(ex, "Failed to deserialize seed data for seeder '{SeederName}'. Invalid JSON format.", seedName);
            return;
        }

        if (models == null || models.Count == 0)
        {
            logger.LogError("No data to seed for seeder '{SeederName}'", seedName);
            return;
        }

        if (!seedingSettings.BatchSettings.SeederBatchSizes.TryGetValue(seedName, out var batchSize)) batchSize = seedingSettings.BatchSettings.DefaultBatchSize;

        logger.LogInformation("Processing {ModelCount} models with batch size {BatchSize}", models.Count, batchSize);

        var existingEntities = await dbContext.Set<TEntity>().ToListAsync(cancellationToken);
        var existingEntitiesDict = existingEntities.ToDictionary(seeder.GetBusinessKeyFromEntity, e => e);

        var processedCount = 0;
        var batchNumber = 1;
        foreach (var batch in models.Batch(batchSize))
        {
            logger.LogInformation("Processing batch {BatchNumber} with {BatchSize} items", batchNumber, batch.Count);

            foreach (var model in batch)
            {
                var businessKey = seeder.GetBusinessKey(model);
                TEntity entityToProcess;

                if (existingEntitiesDict.TryGetValue(businessKey, out var existingEntity))
                {
                    seeder.UpdateEntity(existingEntity, model, referenceResolver);
                    entityToProcess = existingEntity;
                }
                else
                {
                    entityToProcess = seeder.MapEntity(model, referenceResolver);

                    if (entityToProcess is ISystemOwnedEntity systemOwnedEntity) systemOwnedEntity.IsSystemOwned = true;
                    await dbContext.Set<TEntity>().AddAsync(entityToProcess, cancellationToken);
                }

                // Register the entity with the reference resolver if it has a string key
                if (model is IHasKeyModel keyModel) referenceResolver.RegisterEntity(keyModel.Key, entityToProcess);

                processedCount++;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Completed batch {BatchNumber}, processed {ProcessedCount}/{TotalCount} items", batchNumber, processedCount, models.Count);
            batchNumber++;
        }

        if (existingSeedHistory != null)
        {
            existingSeedHistory.ContentHash = hash;
            existingSeedHistory.AppliedDateUtc = DateTime.UtcNow;
        }
        else
        {
            var newSeedHistory = new SeedHistory
            {
                SeedIdentifier = seedName,
                ContentHash = hash,
                AppliedDateUtc = DateTime.UtcNow
            };
            await dbContext.Set<SeedHistory>().AddAsync(newSeedHistory, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Seeder '{SeederName}' completed successfully. Processed {ProcessedCount} items", seedName, processedCount);
    }
}