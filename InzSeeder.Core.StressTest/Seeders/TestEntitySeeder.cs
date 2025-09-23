using InzSeeder.Core.Contracts;
using InzSeeder.Core.StressTest.Data;

namespace InzSeeder.Core.StressTest.Seeders;

/// <summary>
/// Seeder for test entities.
/// </summary>
public class TestEntitySeeder : IEntityDataSeeder<TestEntity, TestEntity>
{
    /// <inheritdoc/>
    public string SeedName => "test-entities";

    /// <inheritdoc/>
    public IEnumerable<Type> Dependencies => [];

    /// <inheritdoc/>
    public object GetBusinessKey(TestEntity model)
    {
        return model.BusinessKey;
    }

    /// <inheritdoc/>
    public object GetBusinessKeyFromEntity(TestEntity entity)
    {
        return entity.BusinessKey;
    }

    /// <inheritdoc/>
    public TestEntity MapEntity(TestEntity model, IEntityReferenceResolver referenceResolver)
    {
        return new TestEntity
        {
            BusinessKey = model.BusinessKey,
            Name = model.Name,
            Description = model.Description,
            Value = model.Value,
            CreatedDate = model.CreatedDate,
            RelatedEntityId = model.RelatedEntityId
        };
    }

    /// <inheritdoc/>
    public void UpdateEntity(TestEntity entity, TestEntity model, IEntityReferenceResolver referenceResolver)
    {
        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.Value = model.Value;
        entity.CreatedDate = model.CreatedDate;
        entity.RelatedEntityId = model.RelatedEntityId;
    }
}