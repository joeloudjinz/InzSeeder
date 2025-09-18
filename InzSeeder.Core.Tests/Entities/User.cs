using InzSeeder.Core.Contracts;

namespace InzSeeder.Core.Tests.Entities;

public class User : ISystemOwnedEntity
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsSystemOwned { get; set; }
}