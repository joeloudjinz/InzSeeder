using InzSeeder.Core.Contracts;
using InzSeeder.Core.Tests.Entities;

namespace InzSeeder.Core.Tests.Seeders;

public class UserSeeder : IEntityDataSeeder<User, UserSeedModel>
{
    public virtual string SeedName => "Users";
    public IEnumerable<Type> Dependencies { get; } = [];

    public object GetBusinessKeyFromEntity(User entity) => entity.Email;
    public object GetBusinessKey(UserSeedModel model) => model.Email;

    public User MapToEntity(UserSeedModel model)
    {
        return new User
        {
            Email = model.Email,
            Name = model.Name
        };
    }

    public void UpdateEntity(User existingEntity, UserSeedModel model)
    {
        existingEntity.Name = model.Name;
    }
}