using InzSeeder.Core.Contracts;
using InzSeeder.Samples.Web.Models;

namespace InzSeeder.Samples.Web.Seeders;

public class ProductCategorySeeder: IEntityDataSeeder<Product, ProductSeedModel>
{
    public string SeedName => "product-categories";

    public IEnumerable<Type> Dependencies => [typeof(CategorySeeder)];

    public object GetBusinessKeyFromEntity(Product entity) => entity.Id;

    public object GetBusinessKey(ProductSeedModel model) => model.Id;

    public Product MapToEntity(ProductSeedModel model)
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

    public void UpdateEntity(Product existingEntity, ProductSeedModel model)
    {
        existingEntity.Name = $"[{model.Id}] {model.Name}";
        existingEntity.Description = model.Description;
        existingEntity.Price = model.Price;
        existingEntity.UpdatedAt = DateTime.UtcNow;
    }
}