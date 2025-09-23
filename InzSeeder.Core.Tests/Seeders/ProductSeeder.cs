using InzSeeder.Core.Contracts;
using InzSeeder.Core.Tests.Entities;

namespace InzSeeder.Core.Tests.Seeders;

public class ProductSeeder : IEntityDataSeeder<Product, ProductSeedModel>
{
    public string SeedName => "Products";
    public IEnumerable<Type> Dependencies { get; } = [];

    public object GetBusinessKeyFromEntity(Product entity) => entity.Key;
    public object GetBusinessKey(ProductSeedModel model) => model.Key;

    public Product MapEntity(ProductSeedModel model, IEntityReferenceResolver referenceResolver)
    {
        return new Product
        {
            Key = model.Key,
            Name = model.Name,
            Sku = model.Sku,
            Price = model.Price
        };
    }

    public void UpdateEntity(Product existingEntity, ProductSeedModel model, IEntityReferenceResolver referenceResolver)
    {
        existingEntity.Key = model.Key;
        existingEntity.Name = model.Name;
        existingEntity.Sku = model.Sku;
        existingEntity.Price = model.Price;
    }
}