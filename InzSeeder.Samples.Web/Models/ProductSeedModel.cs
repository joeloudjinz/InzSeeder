using InzSeeder.Core.Contracts;

namespace InzSeeder.Samples.Web.Models;

public class ProductSeedModel : IHasKeyModel
{
    public string Key { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
}