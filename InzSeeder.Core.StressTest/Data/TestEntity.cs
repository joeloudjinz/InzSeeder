namespace InzSeeder.Core.StressTest.Data;

/// <summary>
/// Test entity for stress testing.
/// </summary>
public class TestEntity
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
    /// Gets or sets the description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the value.
    /// </summary>
    public decimal Value { get; set; }
    
    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// Gets or sets the related entity ID.
    /// </summary>
    public int? RelatedEntityId { get; set; }
    
    /// <summary>
    /// Gets or sets the related entity.
    /// </summary>
    public RelatedTestEntity? RelatedEntity { get; set; }
}