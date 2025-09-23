using InzSeeder.Core.Contracts;

namespace InzSeeder.Samples.Web.Models;

public class UserSeedModel : IHasKeyModel
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
}