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
    private readonly List<Assembly> _assemblies;
    private readonly List<string> _resourceNamespaces;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedResourceSeedDataProvider"/> class.
    /// </summary>
    /// <param name="assemblies">The assemblies to search for seed data resources.</param>
    public EmbeddedResourceSeedDataProvider(params Assembly[] assemblies)
    {
        _assemblies = assemblies.Length > 0 ? assemblies.ToList() : [Assembly.GetExecutingAssembly()];
        _resourceNamespaces = _assemblies.Select(a => $"{a.GetName().Name}.SeedData").ToList();
    }

    /// <inheritdoc/>
    public async Task<(string? content, string? hash)> GetSeedDataAsync(string seedName, CancellationToken cancellationToken)
    {
        // Try to get environment-specific data first
        var environment = EnvironmentUtility.Environment();
        if (!string.IsNullOrEmpty(environment))
        {
            foreach (var (assembly, namespaceName) in _assemblies.Zip(_resourceNamespaces, (a, n) => (a, n)))
            {
                var environmentSpecificResourceName = $"{namespaceName}.{seedName}.{environment}.json";
                var environmentContent = await ReadResourceContentAsync(assembly, environmentSpecificResourceName, cancellationToken);
                if (environmentContent != null)
                {
                    return (environmentContent, ComputeHash(environmentContent));
                }
            }
        }

        // Fall back to default data
        foreach (var (assembly, namespaceName) in _assemblies.Zip(_resourceNamespaces, (a, n) => (a, n)))
        {
            var resourceName = $"{namespaceName}.{seedName}.json";
            var content = await ReadResourceContentAsync(assembly, resourceName, cancellationToken);
            if (content != null)
            {
                return (content, ComputeHash(content));
            }
        }

        // Debug logging - only for the first assembly to avoid verbose output
        if (_assemblies.Count > 0)
        {
            var resourceNames = _assemblies[0].GetManifestResourceNames();
            Console.WriteLine($"Available resources in {_assemblies[0].GetName().Name}: {string.Join(", ", resourceNames)}");
            Console.WriteLine($"Looking for resource: {_resourceNamespaces[0]}.{seedName}.json");
        }
        
        return (null, null);
    }
    
    private async Task<string?> ReadResourceContentAsync(Assembly assembly, string resourceName, CancellationToken cancellationToken)
    {
        await using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;

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