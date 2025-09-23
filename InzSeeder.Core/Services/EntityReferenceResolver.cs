using System.Collections.Concurrent;
using InzSeeder.Core.Contracts;

namespace InzSeeder.Core.Services;

/// <summary>
/// Implementation of IEntityReferenceResolver that manages entity references during the seeding process.
/// </summary>
public class EntityReferenceResolver : IEntityReferenceResolver
{
    // Store registered entities by key (assuming keys are unique across all entities)
    private readonly ConcurrentDictionary<string, object> _entities = new();

    /// <inheritdoc />
    public void RegisterEntity<TEntity>(string key, TEntity entity) where TEntity : class
    {
        _entities[key] = entity;
    }

    /// <inheritdoc />
    public TEntity ResolveEntity<TEntity>(string key) where TEntity : class
    {
        if (_entities.TryGetValue(key, out var entity)) return entity as TEntity ?? throw new Exception($"Failed to cast the entity to {typeof(TEntity).FullName}");

        throw new Exception($"Could not resolve entity with key [{key}].");
    }

    /// <inheritdoc />
    public TIdType ResolveEntityId<TEntity, TIdType>(string key) where TEntity : class
    {
        var entity = ResolveEntity<TEntity>(key);
        if (entity == null) throw new Exception($"Could not resolve entity with key [{key}].");

        // Try to get the ID property using reflection
        var entityType = typeof(TEntity);
        var idProperty = entityType.GetProperty("Id") ?? entityType.GetProperty("ID") ?? entityType.GetProperty("_id") ?? entityType.GetProperty("id");

        return (TIdType)idProperty?.GetValue(entity)! ?? throw new Exception($"Could not resolve the identifier (id, ID, Id, _id) property for entity with key [{key}].");
    }
}