using InzSeeder.Core.Contracts;
using InzSeeder.Samples.Web.Models;

namespace InzSeeder.Samples.Web.Seeders;

public class NotificationTypeSeeder : IEntityDataSeeder<NotificationType, NotificationTypeSeedModel>
{
    public string SeedName => "notification-types";
    public IEnumerable<Type> Dependencies { get; } = [typeof(NotificationTemplateSeeder)];

    public object GetBusinessKeyFromEntity(NotificationType entity) => entity.Key;

    public object GetBusinessKey(NotificationTypeSeedModel model) => model.Key;

    public NotificationType MapEntity(NotificationTypeSeedModel model, IEntityReferenceResolver referenceResolver)
    {
        // Create the entity with basic properties
        var entity = new NotificationType
        {
            Id = Guid.NewGuid(),
            Key = model.Key,
            Name = model.Name,
            Description = model.Description,
            TargetAudience = model.TargetAudience,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Resolve the NotificationTemplate reference if provided
        if (!string.IsNullOrEmpty(model.NotificationTemplateKey))
        {
            entity.NotificationTemplateId = referenceResolver.ResolveEntityId<NotificationTemplate, Guid>(model.NotificationTemplateKey);
        }

        return entity;
    }

    public void UpdateEntity(NotificationType existingEntity, NotificationTypeSeedModel model, IEntityReferenceResolver referenceResolver)
    {
        // Update basic properties
        existingEntity.Name = model.Name;
        existingEntity.Description = model.Description;
        existingEntity.TargetAudience = model.TargetAudience;
        existingEntity.UpdatedAt = DateTime.UtcNow;

        // Resolve the NotificationTemplate reference if provided
        if (!string.IsNullOrEmpty(model.NotificationTemplateKey))
        {
            existingEntity.NotificationTemplateId = referenceResolver.ResolveEntityId<NotificationTemplate, Guid>(model.NotificationTemplateKey);
        }
    }
}