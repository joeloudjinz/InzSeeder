using InzSeeder.Core.Contracts;
using InzSeeder.Samples.Web.Models;

namespace InzSeeder.Samples.Web.Seeders;

public class ProductSeeder : IEntityDataSeeder<Product, ProductSeedModel>
{
    public string SeedName => "products";
    public IEnumerable<Type> Dependencies { get; } = [];

    public object GetBusinessKeyFromEntity(Product entity) => entity.Id;

    public object GetBusinessKey(ProductSeedModel model) => model.Id;

    public Product MapEntity(ProductSeedModel model, IEntityReferenceResolver referenceResolver)
    {
        return new Product
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            Price = model.Price,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void UpdateEntity(Product existingEntity, ProductSeedModel model, IEntityReferenceResolver referenceResolver)
    {
        existingEntity.Name = model.Name;
        existingEntity.Description = model.Description;
        existingEntity.Price = model.Price;
        existingEntity.UpdatedAt = DateTime.UtcNow;
    }
}