using InzSeeder.Core.Contracts;

namespace InzSeeder.Core.Algorithms;

/// <summary>
/// Provides sorting functionality for entity seeders based on their dependencies.
/// </summary>
/// <remarks>
/// This internal static class implements a topological sort algorithm to ensure that seeders
/// are executed in the correct order. It correctly handles dependencies between seeders,
/// preventing issues where a seeder might run before its prerequisites are in place.
/// It also detects circular dependencies and throws an exception to prevent infinite loops.
/// </remarks>
internal static class SeederSorter
{
    /// <summary>
    /// Sorts a collection of seeders based on their dependencies using a topological sort algorithm.
    /// </summary>
    /// <param name="seeders">An enumerable collection of seeders to be sorted.</param>
    /// <returns>A sorted list of seeders, where dependencies appear before the seeders that depend on them.</returns>
    /// <exception cref="InvalidOperationException">Thrown when a circular dependency is detected among the seeders.</exception>
    public static IEnumerable<IBaseEntityDataSeeder> Sort(IEnumerable<IBaseEntityDataSeeder> seeders)
    {
        var seederList = seeders.ToList();
        var sortedSeeders = new List<IBaseEntityDataSeeder>();
        var visited = new HashSet<string>(); // Stores fully visited seeders
        var visiting = new HashSet<string>(); // Stores seeders currently in the recursion stack

        // Create a lookup for quick seeder access by name
        var seederLookup = seederList.ToDictionary(s => s.SeedName);

        foreach (var seeder in seederList)
        {
            Visit(seeder, seederLookup, sortedSeeders, visited, visiting);
        }

        return sortedSeeders;
    }

    /// <summary>
    /// Recursively visits a seeder and its dependencies to perform a depth-first search.
    /// </summary>
    /// <param name="seeder">The current seeder to visit.</param>
    /// <param name="seederLookup">A dictionary for quick lookup of seeders by name.</param>
    /// <param name="sortedSeeders">The list where sorted seeders are collected.</param>
    /// <param name="visited">A set of already visited seeders to avoid redundant processing.</param>
    /// <param name="visiting">A set of seeders currently in the recursion stack to detect circular dependencies.</param>
    private static void Visit(
        IBaseEntityDataSeeder seeder,
        IDictionary<string, IBaseEntityDataSeeder> seederLookup,
        ICollection<IBaseEntityDataSeeder> sortedSeeders,
        ISet<string> visited,
        ISet<string> visiting
    )
    {
        var seederName = seeder.SeedName;

        // If already visited, we can safely skip this seeder.
        if (visited.Contains(seederName)) return;

        // If the seeder is already in the current recursion stack, we have a circular dependency.
        if (!visiting.Add(seederName)) throw new InvalidOperationException($"Circular dependency detected involving seeder '{seederName}'.");

        // Recursively visit all dependencies of the current seeder.
        foreach (var dependencyType in seeder.Dependencies)
        {
            // Find the seeder instance that matches the dependency type.
            var dependencySeeder = seederLookup.Values.FirstOrDefault(s => s.GetType() == dependencyType);
            if (dependencySeeder != null) Visit(dependencySeeder, seederLookup, sortedSeeders, visited, visiting);
        }

        // After visiting all dependencies, remove the seeder from the recursion stack.
        visiting.Remove(seederName);
        // Mark the seeder as fully visited.
        visited.Add(seederName);
        // Add the seeder to the sorted list. It will be added only after all its dependencies are processed.
        sortedSeeders.Add(seeder);
    }
}