using InzSeeder.Core.Contracts;
using InzSeeder.Core.Tests.Entities;

namespace InzSeeder.Core.Tests.Seeders;

public class ProductSeeder : IEntityDataSeeder<Product, ProductSeedModel>
{
    public string SeedName => "Products";
    public IEnumerable<Type> Dependencies { get; } = [];

    public object GetBusinessKeyFromEntity(Product entity) => entity.Sku;
    public object GetBusinessKey(ProductSeedModel model) => model.Sku;

    public Product MapToEntity(ProductSeedModel model)
    {
        return new Product
        {
            Name = model.Name,
            Sku = model.Sku,
            Price = model.Price
        };
    }

    public void UpdateEntity(Product existingEntity, ProductSeedModel model)
    {
        existingEntity.Name = model.Name;
        existingEntity.Price = model.Price;
    }
}