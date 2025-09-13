using InzSeeder.Core.Abstractions;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Samples.InMemory.Models;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Samples.InMemory.Seeders;

public class ProductSeeder : BaseEntitySeeder<Product, ProductSeedModel>
{
    public ProductSeeder(
        ISeedDataProvider seedDataProvider,
        ISeederDbContext dbContext,
        ILogger<ProductSeeder> logger,
        SeedingSettings? seedingSettings = null,
        SeedingPerformanceMetricsService? performanceMetricsService = null
    ) : base(seedDataProvider, dbContext, logger, seedingSettings, performanceMetricsService)
    {
    }

    public override string SeedName => "products";

    protected override object GetBusinessKeyFromEntity(Product entity) => entity.Id;

    protected override object GetBusinessKey(ProductSeedModel model) => model.Id;

    protected override Product MapToEntity(ProductSeedModel model)
    {
        return new Product
        {
            Id = model.Id,
            Name = model.Name,
            Price = model.Price
        };
    }

    protected override void UpdateEntity(Product existingEntity, ProductSeedModel model)
    {
        existingEntity.Name = model.Name;
        existingEntity.Price = model.Price;
    }
}