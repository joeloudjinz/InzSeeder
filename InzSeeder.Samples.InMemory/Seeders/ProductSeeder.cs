using InzSeeder.Core.Contracts;
using InzSeeder.Samples.InMemory.Models;

namespace InzSeeder.Samples.InMemory.Seeders;

public class ProductSeeder : IEntityDataSeeder<Product, ProductSeedModel>
{
    public string SeedName => "products";
    public IEnumerable<Type> Dependencies { get; } = [];
    public object GetBusinessKeyFromEntity(Product entity) => entity.Id;
    public object GetBusinessKey(ProductSeedModel model) => model.Id;

    public Product MapToEntity(ProductSeedModel model)
    {
        return new Product
        {
            Id = model.Id,
            Name = model.Name,
            Price = model.Price
        };
    }

    public void UpdateEntity(Product existingEntity, ProductSeedModel model)
    {
        existingEntity.Name = model.Name;
        existingEntity.Price = model.Price;
    }
}