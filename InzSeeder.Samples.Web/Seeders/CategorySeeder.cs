using InzSeeder.Core.Abstractions;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Samples.Web.Models;

namespace InzSeeder.Samples.Web.Seeders;

public class CategorySeeder(
    ISeedDataProvider seedDataProvider,
    ISeederDbContext dbContext,
    ILogger<CategorySeeder> logger,
    SeederConfiguration? seedingSettings = null,
    SeedingPerformanceMetricsService? performanceMetricsService = null)
    : BaseEntitySeeder<Category, CategorySeedModel>(seedDataProvider, dbContext, logger, seedingSettings, performanceMetricsService)
{
    public override string SeedName => "categories";

    protected override object GetBusinessKeyFromEntity(Category entity) => entity.Id;

    protected override object GetBusinessKey(CategorySeedModel model) => model.Id;

    protected override Category MapToEntity(CategorySeedModel model)
    {
        return new Category
        {
            Id = model.Id,
            Name = model.Name,
            Slug = model.Slug,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    protected override void UpdateEntity(Category existingEntity, CategorySeedModel model)
    {
        existingEntity.Name = model.Name;
        existingEntity.Slug = model.Slug;
        existingEntity.IsActive = model.IsActive;
        existingEntity.UpdatedAt = DateTime.UtcNow;
    }
}