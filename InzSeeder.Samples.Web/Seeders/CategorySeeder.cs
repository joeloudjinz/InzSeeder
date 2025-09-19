using InzSeeder.Core.Contracts;
using InzSeeder.Samples.Web.Models;

namespace InzSeeder.Samples.Web.Seeders;

public class CategorySeeder: IEntityDataSeeder<Category, CategorySeedModel>
{
    public string SeedName => "categories";
    public IEnumerable<Type> Dependencies { get; } = [];

    public object GetBusinessKeyFromEntity(Category entity) => entity.Id;

    public object GetBusinessKey(CategorySeedModel model) => model.Id;

    public Category MapToEntity(CategorySeedModel model)
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

    public void UpdateEntity(Category existingEntity, CategorySeedModel model)
    {
        existingEntity.Name = model.Name;
        existingEntity.Slug = model.Slug;
        existingEntity.IsActive = model.IsActive;
        existingEntity.UpdatedAt = DateTime.UtcNow;
    }
}