using InzSeeder.Core.Contracts;

namespace InzSeeder.Core.Tests.Entities;

public class Product : ISystemOwnedEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool IsSystemOwned { get; set; }
}