namespace InzSeeder.Core.Tests.Entities;

public class UserProfileSeedModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Bio { get; set; } = string.Empty;
}