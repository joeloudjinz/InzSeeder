using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Utilities;

namespace InzSeeder.Core.Services;

/// <summary>
/// Provides seed data from embedded resources.
/// </summary>
public class EmbeddedResourceSeedDataProvider : ISeedDataProvider
{
    private readonly Assembly[] _assemblies;
    private readonly string[] _resourceNamespaces;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedResourceSeedDataProvider"/> class.
    /// </summary>
    /// <param name="assemblies">The assemblies to search for seed data resources.</param>
    public EmbeddedResourceSeedDataProvider(params Assembly[] assemblies)
    {
        _assemblies = assemblies.Length > 0 ? assemblies : [Assembly.GetExecutingAssembly()];
        _resourceNamespaces = _assemblies.Select(a => $"{a.GetName().Name}.SeedData").ToArray();
    }

    /// <inheritdoc/>
    public async Task<(string? content, string? hash)> GetSeedDataAsync(string seedName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(seedName);

        // Try to get environment-specific data first
        var environment = EnvironmentUtility.Environment();
        if (string.IsNullOrEmpty(environment)) return await TryGetSeedDataAsync(seedName, null, cancellationToken);

        var result = await TryGetSeedDataAsync(seedName, environment, cancellationToken);
        if (result.content is not null) return result;

        // Fall back to default data
        return await TryGetSeedDataAsync(seedName, null, cancellationToken);
    }

    private async Task<(string? content, string? hash)> TryGetSeedDataAsync(string seedName, string? environment, CancellationToken cancellationToken)
    {
        // Precompute the suffix patterns we're looking for
        var environmentSuffix = !string.IsNullOrEmpty(environment) ? $".{seedName}.{environment}.json" : string.Empty;
        var defaultSuffix = $".{seedName}.json";

        // Process each assembly
        for (var i = 0; i < _assemblies.Length; i++)
        {
            var assembly = _assemblies[i];
            var namespaceName = _resourceNamespaces[i];

            // First try direct match (for files in SeedData root)
            var directResourceName = string.IsNullOrEmpty(environment)
                ? $"{namespaceName}.{seedName}.json"
                : $"{namespaceName}.{seedName}.{environment}.json";

            var content = await ReadResourceContentAsync(assembly, directResourceName, cancellationToken);
            if (content is not null) return (content, ComputeHash(content));

            // If not found, try to find in subdirectories by enumerating resources once
            var resourceNames = assembly.GetManifestResourceNames();

            // Look for the first matching resource
            foreach (var resourceName in resourceNames)
            {
                // Check if it matches our namespace prefix
                if (!resourceName.StartsWith($"{namespaceName}.")) continue;

                // Check if it matches our suffix patterns
                if (!string.IsNullOrEmpty(environment))
                {
                    // Looking for environment-specific resource
                    if (!resourceName.EndsWith(environmentSuffix)) continue;
                }
                else
                {
                    // Looking for default resource, but not an environment-specific one
                    if (!resourceName.EndsWith(defaultSuffix) || (!string.IsNullOrEmpty(environment) && resourceName.EndsWith($".{environment}.{seedName}.json"))) continue;
                }

                content = await ReadResourceContentAsync(assembly, resourceName, cancellationToken);
                if (content is not null) return (content, ComputeHash(content));
            }
        }

        return (null, null);
    }

    private async Task<string?> ReadResourceContentAsync(Assembly assembly, string resourceName, CancellationToken cancellationToken)
    {
        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null) return null;

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static string ComputeHash(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}