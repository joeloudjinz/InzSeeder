using InzSeeder.Core.Abstractions;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Core.Tests.Entities;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Tests.Seeders;

public class UserProfileSeeder(
    ISeedDataProvider seedDataProvider,
    ISeederDbContext dbContext,
    ILogger<UserProfileSeeder> logger,
    SeederConfiguration? seedingSettings = null,
    SeedingPerformanceMetricsService? performanceMetricsService = null)
    : BaseEntitySeeder<UserProfile, UserProfileSeedModel>(seedDataProvider, dbContext, logger, seedingSettings, performanceMetricsService)
{
    public override string SeedName => "UserProfiles";

    public override IEnumerable<Type> Dependencies => [typeof(UserSeeder)];

    protected override object GetBusinessKey(UserProfileSeedModel model) => model.UserId;

    protected override object GetBusinessKeyFromEntity(UserProfile entity) => entity.UserId;

    protected override UserProfile MapToEntity(UserProfileSeedModel model) => new()
    {
        UserId = model.UserId,
        Bio = model.Bio
    };

    protected override void UpdateEntity(UserProfile existingEntity, UserProfileSeedModel model)
    {
        existingEntity.Bio = model.Bio;
    }
}