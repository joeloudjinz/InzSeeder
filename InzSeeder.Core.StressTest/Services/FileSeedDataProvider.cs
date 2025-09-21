using System.Security.Cryptography;
using System.Text;
using InzSeeder.Core.Contracts;

namespace InzSeeder.Core.StressTest.Services;

/// <summary>
/// Provides seed data from files in the SeedData directory.
/// </summary>
public class FileSeedDataProvider : ISeedDataProvider
{
    private readonly string _seedDataDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSeedDataProvider"/> class.
    /// </summary>
    /// <param name="seedDataDirectory">The directory containing seed data files.</param>
    public FileSeedDataProvider(string seedDataDirectory)
    {
        _seedDataDirectory = seedDataDirectory;
    }

    /// <inheritdoc/>
    public async Task<(string? content, string? hash)> GetSeedDataAsync(string seedName, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(seedName);

        // Try to get environment-specific data first
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (string.IsNullOrEmpty(environment)) 
            return await TryGetSeedDataAsync(seedName, null, cancellationToken);

        var result = await TryGetSeedDataAsync(seedName, environment, cancellationToken);
        if (result.content is not null) return result;

        // Fall back to default data
        return await TryGetSeedDataAsync(seedName, null, cancellationToken);
    }

    private async Task<(string? content, string? hash)> TryGetSeedDataAsync(string seedName, string? environment, CancellationToken cancellationToken)
    {
        // Precompute the file names we're looking for
        var fileName = string.IsNullOrEmpty(environment)
            ? $"{seedName}.json"
            : $"{seedName}.{environment}.json";

        var filePath = Path.Combine(_seedDataDirectory, fileName);

        // Check if file exists
        if (!File.Exists(filePath)) 
            return (null, null);

        // Read file content
        var content = await File.ReadAllTextAsync(filePath, cancellationToken);
        return (content, ComputeHash(content));
    }

    private static string ComputeHash(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}