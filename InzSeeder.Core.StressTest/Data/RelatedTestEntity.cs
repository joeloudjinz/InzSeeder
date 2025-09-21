namespace InzSeeder.Core.StressTest.Data;

/// <summary>
/// Related test entity for stress testing relationships.
/// </summary>
public class RelatedTestEntity
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the business key used for identifying existing entities.
    /// </summary>
    public string BusinessKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the category.
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the priority.
    /// </summary>
    public int Priority { get; set; }
}