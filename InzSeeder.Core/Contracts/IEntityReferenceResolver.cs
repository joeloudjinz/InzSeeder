namespace InzSeeder.Core.Contracts;

/// <summary>
/// Provides a mechanism to register and resolve entity references during the seeding process.
/// This service allows seeders to reference entities that are created during seeding by using
/// string-based keys instead of hardcoded GUIDs.
/// </summary>
public interface IEntityReferenceResolver
{
    /// <summary>
    /// Registers an entity with a string key that can be used to reference it later.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="key">The string key to associate with the entity.</param>
    /// <param name="entity">The entity to register.</param>
    /// <exception cref="Exception">Thrown when the key is null or empty.</exception>
    void RegisterEntity<TEntity>(string key, TEntity entity) where TEntity : class;

    /// <summary>
    /// Resolves an entity by its key.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="key">The string key associated with the entity.</param>
    /// <returns>The resolved entity.</returns>
    /// <exception cref="Exception">Thrown when the key is null or empty, when the entity with the specified key cannot be found, or when the entity cannot be cast to the requested type.</exception>
    TEntity ResolveEntity<TEntity>(string key) where TEntity : class;

    /// <summary>
    /// Resolves an entity's ID by its key.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TIdType">The type of the id</typeparam>
    /// <param name="key">The string key associated with the entity.</param>
    /// <returns>The ID of the resolved entity.</returns>
    /// <exception cref="Exception">Thrown when the key is null or empty, when the entity with the specified key cannot be found, 
    /// when the entity cannot be cast to the requested type, or when the entity does not have an 'Id', 'id', '_id' or 'ID' property.</exception>
    TIdType ResolveEntityId<TEntity, TIdType>(string key) where TEntity : class;

    /// <summary>
    /// Resolves a Guid ID of an entity by its key.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="key">The string key associated with the entity.</param>
    /// <returns>The Guid ID of the resolved entity.</returns>
    /// <exception cref="Exception">Thrown when the key is null or empty, when the entity with the specified key cannot be found,
    /// when the entity cannot be cast to the requested type, or when the entity does not have an 'Id', 'id', '_id' or 'ID' property of type Guid.</exception>
    Guid ResolveGuidId<TEntity>(string key) where TEntity : class;

    /// <summary>
    /// Resolves a string ID of an entity by its key.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="key">The string key associated with the entity.</param>
    /// <returns>The string ID of the resolved entity.</returns>
    /// <exception cref="Exception">Thrown when the key is null or empty, when the entity with the specified key cannot be found,
    /// when the entity cannot be cast to the requested type, or when the entity does not have an 'Id', 'id', '_id' or 'ID' property of type string.</exception>
    string ResolveStringId<TEntity>(string key) where TEntity : class;

    /// <summary>
    /// Resolves an integer ID of an entity by its key.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <param name="key">The string key associated with the entity.</param>
    /// <returns>The integer ID of the resolved entity.</returns>
    /// <exception cref="Exception">Thrown when the key is null or empty, when the entity with the specified key cannot be found,
    /// when the entity cannot be cast to the requested type, or when the entity does not have an 'Id', 'id', '_id' or 'ID' property of type int.</exception>
    int ResolveIntId<TEntity>(string key) where TEntity : class;
}