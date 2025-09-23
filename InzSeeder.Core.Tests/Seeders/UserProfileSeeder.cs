using InzSeeder.Core.Contracts;
using InzSeeder.Core.Tests.Entities;

namespace InzSeeder.Core.Tests.Seeders;

public class UserProfileSeeder : IEntityDataSeeder<UserProfile, UserProfileSeedModel>
{
    public string SeedName => "UserProfiles";
    public IEnumerable<Type> Dependencies => [typeof(UserSeeder)];

    public object GetBusinessKeyFromEntity(UserProfile entity) => entity.Key;
    public object GetBusinessKey(UserProfileSeedModel model) => model.Key;

    public UserProfile MapEntity(UserProfileSeedModel model, IEntityReferenceResolver referenceResolver)
    {
        return new UserProfile
        {
            Key = model.Key,
            UserId = model.UserId,
            Bio = model.Bio
        };
    }

    public void UpdateEntity(UserProfile existingEntity, UserProfileSeedModel model, IEntityReferenceResolver referenceResolver)
    {
        existingEntity.UserId = model.UserId;
        existingEntity.Bio = model.Bio;
    }
}