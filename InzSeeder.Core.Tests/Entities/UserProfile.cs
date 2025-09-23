namespace InzSeeder.Core.Tests.Entities;

public class UserProfile
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Bio { get; set; } = string.Empty;
    public User? User { get; set; }
}