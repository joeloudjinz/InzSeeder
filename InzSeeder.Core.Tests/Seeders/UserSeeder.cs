using InzSeeder.Core.Contracts;
using InzSeeder.Core.Tests.Entities;

namespace InzSeeder.Core.Tests.Seeders;

public class UserSeeder : IEntityDataSeeder<User, UserSeedModel>
{
    public virtual string SeedName => "Users";
    public IEnumerable<Type> Dependencies { get; } = [];

    public object GetBusinessKeyFromEntity(User entity) => entity.Key;
    public object GetBusinessKey(UserSeedModel model) => model.Key;

    public User MapEntity(UserSeedModel model, IEntityReferenceResolver referenceResolver)
    {
        return new User
        {
            Key = model.Key,
            Email = model.Email,
            Name = model.Name
        };
    }

    public void UpdateEntity(User existingEntity, UserSeedModel model, IEntityReferenceResolver referenceResolver)
    {
        existingEntity.Email = model.Email;
        existingEntity.Name = model.Name;
    }
}