using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using InzSeeder.Core.Contracts;

namespace InzSeeder.Core.Services;

/// <summary>
/// Provides seed data from embedded resources.
/// </summary>
public class EmbeddedResourceSeedDataProvider : ISeedDataProvider
{
    private readonly Assembly _assembly;
    private readonly string _resourceNamespace;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EmbeddedResourceSeedDataProvider"/> class.
    /// </summary>
    public EmbeddedResourceSeedDataProvider()
    {
        _assembly = Assembly.GetExecutingAssembly();
        _resourceNamespace = $"{_assembly.GetName().Name}.SeedData";
    }

    /// <inheritdoc/>
    public async Task<(string? content, string? hash)> GetSeedDataAsync(string seedName, CancellationToken cancellationToken)
    {
        var resourceName = $"{_resourceNamespace}.{seedName}.json";
        
        await using var stream = _assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            // Let's log the available resources for debugging
            var resourceNames = _assembly.GetManifestResourceNames();
            Console.WriteLine($"Available resources: {string.Join(", ", resourceNames)}");
            Console.WriteLine($"Looking for resource: {resourceName}");
            return (null, null);
        }

        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync(cancellationToken);
        
        var hash = ComputeHash(content);
        return (content, hash);
    }
    
    private static string ComputeHash(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}