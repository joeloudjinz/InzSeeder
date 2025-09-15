using InzSeeder.Core.Abstractions;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Samples.Web.Models;

namespace InzSeeder.Samples.Web.Seeders;

public class ProductCategorySeeder(
    ISeedDataProvider seedDataProvider,
    ISeederDbContext dbContext,
    ILogger<ProductCategorySeeder> logger,
    SeederConfiguration? seedingSettings = null,
    SeedingPerformanceMetricsService? performanceMetricsService = null
) : BaseEntitySeeder<Product, ProductSeedModel>(seedDataProvider, dbContext, logger, seedingSettings, performanceMetricsService)
{
    public override string SeedName => "product-categories";

    public override IEnumerable<Type> Dependencies => [typeof(CategorySeeder)];

    protected override object GetBusinessKeyFromEntity(Product entity) => entity.Id;

    protected override object GetBusinessKey(ProductSeedModel model) => model.Id;

    protected override Product MapToEntity(ProductSeedModel model)
    {
        return new Product
        {
            Id = model.Id,
            Name = $"[{model.Id}] {model.Name}",
            Description = model.Description,
            Price = model.Price,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    protected override void UpdateEntity(Product existingEntity, ProductSeedModel model)
    {
        existingEntity.Name = $"[{model.Id}] {model.Name}";
        existingEntity.Description = model.Description;
        existingEntity.Price = model.Price;
        existingEntity.UpdatedAt = DateTime.UtcNow;
    }
}