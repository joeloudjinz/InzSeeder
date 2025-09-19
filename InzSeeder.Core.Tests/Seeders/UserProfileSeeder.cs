using InzSeeder.Core.Contracts;
using InzSeeder.Core.Tests.Entities;

namespace InzSeeder.Core.Tests.Seeders;

public class UserProfileSeeder : IEntityDataSeeder<UserProfile, UserProfileSeedModel>
{
    public string SeedName => "UserProfiles";
    public IEnumerable<Type> Dependencies => [typeof(UserSeeder)];

    public object GetBusinessKeyFromEntity(UserProfile entity) => entity.UserId;
    public object GetBusinessKey(UserProfileSeedModel model) => model.UserId;

    public UserProfile MapToEntity(UserProfileSeedModel model)
    {
        return new UserProfile
        {
            UserId = model.UserId,
            Bio = model.Bio
        };
    }

    public void UpdateEntity(UserProfile existingEntity, UserProfileSeedModel model)
    {
        existingEntity.Bio = model.Bio;
    }
}