using InzSeeder.Core.Contracts;
using InzSeeder.Core.StressTest.Data;

namespace InzSeeder.Core.StressTest.Seeders;

/// <summary>
/// Seeder for related test entities.
/// </summary>
public class RelatedTestEntitySeeder : IEntityDataSeeder<RelatedTestEntity, RelatedTestEntity>
{
    /// <inheritdoc/>
    public string SeedName => "related-test-entities";

    /// <inheritdoc/>
    public IEnumerable<Type> Dependencies => [];

    /// <inheritdoc/>
    public object GetBusinessKey(RelatedTestEntity model)
    {
        return model.BusinessKey;
    }

    /// <inheritdoc/>
    public object GetBusinessKeyFromEntity(RelatedTestEntity entity)
    {
        return entity.BusinessKey;
    }

    /// <inheritdoc/>
    public RelatedTestEntity MapEntity(RelatedTestEntity model, IEntityReferenceResolver referenceResolver)
    {
        return new RelatedTestEntity
        {
            BusinessKey = model.BusinessKey,
            Name = model.Name,
            Category = model.Category,
            Priority = model.Priority
        };
    }

    /// <inheritdoc/>
    public void UpdateEntity(RelatedTestEntity entity, RelatedTestEntity model, IEntityReferenceResolver referenceResolver)
    {
        entity.Name = model.Name;
        entity.Category = model.Category;
        entity.Priority = model.Priority;
    }
}