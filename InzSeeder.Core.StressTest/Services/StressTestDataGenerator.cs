using InzSeeder.Core.StressTest.Data;

namespace InzSeeder.Core.StressTest.Services;

/// <summary>
/// Service for generating stress test data.
/// </summary>
public class StressTestDataGenerator
{
    private static readonly Random Random = new();
    private static readonly string[] Adjectives = ["Fast", "Quick", "Rapid", "Swift", "Speedy", "Hasty", "Brisk", "Fleet", "Snappy", "Zippy"];
    private static readonly string[] Nouns = ["Processor", "Engine", "Machine", "Device", "System", "Unit", "Module", "Component", "Element", "Part"];
    private static readonly string[] Categories = ["Electronics", "Hardware", "Software", "Network", "Storage", "Security", "Database", "Cloud", "Mobile", "Web"];

    private static readonly string[] Descriptions =
    [
        "High-performance component for demanding applications",
        "Reliable and efficient solution for enterprise environments",
        "Scalable architecture designed for modern workloads",
        "Optimized for maximum throughput and minimal latency",
        "Advanced technology with cutting-edge features",
        "Industry-leading performance and reliability",
        "Future-proof design with extensive compatibility",
        "Robust implementation with comprehensive error handling",
        "Lightweight yet powerful for resource-constrained environments",
        "Modular design allowing for flexible configuration"
    ];

    /// <summary>
    /// Generates a collection of test entities.
    /// </summary>
    /// <param name="count">The number of entities to generate.</param>
    /// <returns>A collection of test entities.</returns>
    public IEnumerable<TestEntity> GenerateTestEntities(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return new TestEntity
            {
                BusinessKey = $"TEST-{i:D6}",
                Name = $"{GetRandomElement(Adjectives)} {GetRandomElement(Nouns)} #{i}",
                Description = GetRandomElement(Descriptions),
                Value = Random.Next(100, 10000) / 100m,
                CreatedDate = DateTime.UtcNow.AddDays(-Random.Next(365)),
                RelatedEntityId = Random.Next(1, Math.Max(1, count / 10)) // 10% have related entities
            };
        }
    }

    /// <summary>
    /// Generates a collection of related test entities.
    /// </summary>
    /// <param name="count">The number of entities to generate.</param>
    /// <returns>A collection of related test entities.</returns>
    public IEnumerable<RelatedTestEntity> GenerateRelatedTestEntities(int count)
    {
        for (var i = 0; i < count; i++)
        {
            yield return new RelatedTestEntity
            {
                BusinessKey = $"RELATED-{i:D5}",
                Name = $"{GetRandomElement(Nouns)} Category #{i}",
                Category = GetRandomElement(Categories),
                Priority = Random.Next(1, 10)
            };
        }
    }

    private static T GetRandomElement<T>(T[] array)
    {
        return array[Random.Next(array.Length)];
    }
}