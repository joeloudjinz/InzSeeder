using InzSeeder.Core.Abstractions;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Core.Tests.Entities;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Tests.Seeders;

public class UserSeeder(
    ISeedDataProvider seedDataProvider,
    ISeederDbContext dbContext,
    ILogger<UserSeeder> logger,
    SeederConfiguration? seedingSettings = null,
    SeedingPerformanceMetricsService? performanceMetricsService = null)
    : BaseEntitySeeder<User, UserSeedModel>(seedDataProvider, dbContext, logger, seedingSettings, performanceMetricsService)
{
    public override string SeedName => "Users";

    protected override object GetBusinessKey(UserSeedModel model) => model.Email;

    protected override object GetBusinessKeyFromEntity(User entity) => entity.Email;

    protected override User MapToEntity(UserSeedModel model) => new()
    {
        Email = model.Email,
        Name = model.Name
    };

    protected override void UpdateEntity(User existingEntity, UserSeedModel model)
    {
        existingEntity.Name = model.Name;
    }
}