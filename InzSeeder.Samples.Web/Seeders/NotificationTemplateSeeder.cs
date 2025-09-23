using InzSeeder.Core.Contracts;
using InzSeeder.Samples.Web.Models;

namespace InzSeeder.Samples.Web.Seeders;

public class NotificationTemplateSeeder : IEntityDataSeeder<NotificationTemplate, NotificationTemplateSeedModel>
{
    public string SeedName => "notification-templates";
    public IEnumerable<Type> Dependencies { get; } = [];

    public object GetBusinessKeyFromEntity(NotificationTemplate entity) => entity.Key;

    public object GetBusinessKey(NotificationTemplateSeedModel model) => model.Key;

    public NotificationTemplate MapEntity(NotificationTemplateSeedModel model, IEntityReferenceResolver _)
    {
        // Create the entity with basic properties
        var entity = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Key = model.Key,
            Name = model.Name,
            Subject = model.Subject,
            Body = model.Body,
            DeliveryMethod = model.DeliveryMethod,
            IsMasterLayout = model.IsMasterLayout,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return entity;
    }

    public void UpdateEntity(NotificationTemplate existingEntity, NotificationTemplateSeedModel model, IEntityReferenceResolver _)
    {
        // Update basic properties
        existingEntity.Name = model.Name;
        existingEntity.Subject = model.Subject;
        existingEntity.Body = model.Body;
        existingEntity.DeliveryMethod = model.DeliveryMethod;
        existingEntity.IsMasterLayout = model.IsMasterLayout;
        existingEntity.UpdatedAt = DateTime.UtcNow;
    }
}