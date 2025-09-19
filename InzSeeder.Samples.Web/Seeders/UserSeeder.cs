using InzSeeder.Core.Contracts;
using InzSeeder.Samples.Web.Models;

namespace InzSeeder.Samples.Web.Seeders;

public class UserSeeder: IEntityDataSeeder<User, UserSeedModel>
{
    public string SeedName => "users";
    public IEnumerable<Type> Dependencies { get; } = [];

    public object GetBusinessKeyFromEntity(User entity) => entity.Id;

    public object GetBusinessKey(UserSeedModel model) => model.Id;

    public User MapToEntity(UserSeedModel model)
    {
        return new User
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Email = model.Email,
            DateOfBirth = model.DateOfBirth,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateEntity(User existingEntity, UserSeedModel model)
    {
        existingEntity.FirstName = model.FirstName;
        existingEntity.LastName = model.LastName;
        existingEntity.Email = model.Email;
        existingEntity.DateOfBirth = model.DateOfBirth;
        existingEntity.UpdatedAt = DateTime.UtcNow;
    }
}