using InzSeeder.Core.Contracts;

namespace InzSeeder.Samples.Web.Models;

public class NotificationTemplateSeedModel : IHasKeyModel
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Subject { get; set; }
    public string Body { get; set; } = string.Empty;
    public string DeliveryMethod { get; set; } = string.Empty;
    public bool IsMasterLayout { get; set; }
}