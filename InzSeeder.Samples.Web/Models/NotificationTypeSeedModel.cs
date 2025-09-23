using InzSeeder.Core.Contracts;

namespace InzSeeder.Samples.Web.Models;

public class NotificationTypeSeedModel : IHasKeyModel
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TargetAudience { get; set; } = string.Empty;
    public string? NotificationTemplateKey { get; set; }
}