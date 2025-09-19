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

public interface IEntityDataSeeder<TEntity, in TModel>: IBaseEntityDataSeeder
{
    /// <summary>
    /// Gets the business key from an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The business key.</returns>
    public object GetBusinessKeyFromEntity(TEntity entity);

    /// <summary>
    /// Gets the business key from a model.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The business key.</returns>
    public object GetBusinessKey(TModel model);

    /// <summary>
    /// Maps a model to an entity.
    /// </summary>
    /// <param name="model">The model.</param>
    /// <returns>The entity.</returns>
    public TEntity MapToEntity(TModel model);

    /// <summary>
    /// Updates an existing entity with data from a model.
    /// </summary>
    /// <param name="existingEntity">The existing entity.</param>
    /// <param name="model">The model.</param>
    public void UpdateEntity(TEntity existingEntity, TModel model);
}