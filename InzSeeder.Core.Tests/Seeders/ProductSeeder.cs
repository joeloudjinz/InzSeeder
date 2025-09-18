using InzSeeder.Core.Abstractions;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Core.Tests.Entities;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Tests.Seeders;

public class ProductSeeder(
    ISeedDataProvider seedDataProvider,
    ISeederDbContext dbContext,
    ILogger<ProductSeeder> logger,
    SeederConfiguration? seedingSettings = null,
    SeedingPerformanceMetricsService? performanceMetricsService = null)
    : BaseEntitySeeder<Product, ProductSeedModel>(seedDataProvider, dbContext, logger, seedingSettings, performanceMetricsService)
{
    public override string SeedName => "Products";

    protected override object GetBusinessKey(ProductSeedModel model) => model.Sku;

    protected override object GetBusinessKeyFromEntity(Product entity) => entity.Sku;

    protected override Product MapToEntity(ProductSeedModel model) => new()
    {
        Name = model.Name,
        Sku = model.Sku,
        Price = model.Price
    };

    protected override void UpdateEntity(Product existingEntity, ProductSeedModel model)
    {
        existingEntity.Name = model.Name;
        existingEntity.Price = model.Price;
    }
}