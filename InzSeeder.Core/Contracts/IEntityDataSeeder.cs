namespace InzSeeder.Core.Contracts;

public interface IBaseEntityDataSeeder
{
    /// <summary>
    /// Gets the unique name of this seeder.
    /// </summary>
    string SeedName { get; }

    /// <summary>
    /// Gets the collection of seeder types that this seeder depends on.
    /// </summary>
    IEnumerable<Type> Dependencies { get; }
}

public interface IEntityDataSeeder<TEntity, in TModel> : IBaseEntityDataSeeder
{
    /// <summary>
    /// Gets the business key from an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The business key.</returns>
    /// TODO remove the feature of making seeders implement this method and use Key or Id properties directly 
    public object GetBusinessKeyFromEntity(TEntity entity);

    /// <summary>
    /// Gets the business key from a model.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The business key.</returns>
    /// TODO remove the feature of making seeders implement this method and use Key or Id properties directly
    public object GetBusinessKey(TModel model);

    /// <summary>
    /// Maps a model to an entity with support for resolving entity references.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <param name="referenceResolver">The reference resolver service.</param>
    /// <returns>The entity.</returns>
    public TEntity MapEntity(TModel model, IEntityReferenceResolver referenceResolver);

    /// <summary>
    /// Updates an existing entity with data from a model with support for resolving entity references.
    /// </summary>
    /// <param name="existingEntity">The existing entity.</param>
    /// <param name="model">The model.</param>
    /// <param name="referenceResolver">The reference resolver service.</param>
    public void UpdateEntity(TEntity existingEntity, TModel model, IEntityReferenceResolver referenceResolver);
}