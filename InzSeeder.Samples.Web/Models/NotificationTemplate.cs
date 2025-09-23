namespace InzSeeder.Samples.Web.Models;

public class NotificationTemplate
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public string DeliveryMethod { get; set; }
    public bool IsMasterLayout { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public int? LastModifiedByUserId { get; set; }
    public User? LastModifiedByUser { get; set; }
    
    public NotificationType? NotificationType { get; set; }
}