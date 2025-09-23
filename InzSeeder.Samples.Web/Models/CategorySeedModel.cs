using InzSeeder.Core.Contracts;

namespace InzSeeder.Samples.Web.Models;

public class CategorySeedModel : IHasKeyModel
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}