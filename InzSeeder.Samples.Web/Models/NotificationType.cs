namespace InzSeeder.Samples.Web.Models;

public class NotificationType
{
    public Guid Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TargetAudience { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public int? LastModifiedByUserId { get; set; }
    public User? LastModifiedByUser { get; set; }

    public Guid NotificationTemplateId { get; set; }
    public NotificationTemplate NotificationTemplate { get; set; }
}