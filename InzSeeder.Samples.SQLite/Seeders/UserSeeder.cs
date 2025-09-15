using InzSeeder.Core.Abstractions;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Samples.SQLite.Models;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Samples.SQLite.Seeders;

public class UserSeeder : BaseEntitySeeder<User, UserSeedModel>
{
    public UserSeeder(
        ISeedDataProvider seedDataProvider,
        ISeederDbContext dbContext,
        ILogger<UserSeeder> logger,
        SeederConfiguration? seedingSettings = null,
        SeedingPerformanceMetricsService? performanceMetricsService = null
    ) : base(seedDataProvider, dbContext, logger, seedingSettings, performanceMetricsService)
    {
    }

    public override string SeedName => "users";

    protected override object GetBusinessKeyFromEntity(User entity) => entity.Id;

    protected override object GetBusinessKey(UserSeedModel model) => model.Id;

    protected override User MapToEntity(UserSeedModel model)
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

    protected override void UpdateEntity(User existingEntity, UserSeedModel model)
    {
        existingEntity.FirstName = model.FirstName;
        existingEntity.LastName = model.LastName;
        existingEntity.Email = model.Email;
        existingEntity.DateOfBirth = model.DateOfBirth;
        existingEntity.UpdatedAt = DateTime.UtcNow;
    }
}