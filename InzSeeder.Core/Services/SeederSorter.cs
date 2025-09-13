using InzSeeder.Core.Contracts;

namespace InzSeeder.Core.Services;

/// <summary>
/// Provides sorting functionality for entity seeders based on their dependencies.
/// </summary>
public static class SeederSorter
{
    /// <summary>
    /// Sorts the given collection of entity seeders based on their dependencies using topological sorting.
    /// </summary>
    /// <param name="seeders">The collection of entity seeders to sort.</param>
    /// <returns>A sorted collection of entity seeders.</returns>
    /// <exception cref="InvalidOperationException">Thrown when circular dependencies are detected.</exception>
    public static IEnumerable<IEntitySeeder> Sort(IEnumerable<IEntitySeeder> seeders)
    {
        var seederList = seeders.ToList();
        var sortedSeeders = new List<IEntitySeeder>();
        var visited = new HashSet<string>();
        var visiting = new HashSet<string>();

        // Create a lookup for quick seeder access by name
        var seederLookup = seederList.ToDictionary(s => s.SeedName);

        foreach (var seeder in seederList)
        {
            Visit(seeder, seederLookup, sortedSeeders, visited, visiting);
        }

        return sortedSeeders;
    }

    /// <summary>
    /// Recursively visits a seeder and its dependencies to perform topological sorting.
    /// </summary>
    /// <param name="seeder">The seeder to visit.</param>
    /// <param name="seederLookup">A lookup of seeders by name.</param>
    /// <param name="sortedSeeders">The list to add sorted seeders to.</param>
    /// <param name="visited">A set of already visited seeder names.</param>
    /// <param name="visiting">A set of currently visiting seeder names (to detect circular dependencies).</param>
    /// <exception cref="InvalidOperationException">Thrown when circular dependencies are detected.</exception>
    private static void Visit(
        IEntitySeeder seeder,
        IDictionary<string, IEntitySeeder> seederLookup,
        ICollection<IEntitySeeder> sortedSeeders,
        ISet<string> visited,
        ISet<string> visiting
    )
    {
        var seederName = seeder.SeedName;

        // If already visited, skip
        if (visited.Contains(seederName)) return;

        // If currently visiting, we have a circular dependency, otherwise, mark as visiting
        if (!visiting.Add(seederName)) throw new InvalidOperationException($"Circular dependency detected involving seeder '{seederName}'.");

        // Visit dependencies first
        foreach (var dependencyType in seeder.Dependencies)
        {
            // Find the seeder that matches this dependency type
            var dependencySeeder = seederLookup.Values.FirstOrDefault(s => s.GetType() == dependencyType);
            if (dependencySeeder != null) Visit(dependencySeeder, seederLookup, sortedSeeders, visited, visiting);
        }

        // Mark as visited and add to sorted list
        visiting.Remove(seederName);
        visited.Add(seederName);
        sortedSeeders.Add(seeder);
    }
}