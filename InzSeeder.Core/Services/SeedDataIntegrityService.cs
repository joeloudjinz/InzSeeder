using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Services;

/// <summary>
/// Service for verifying the integrity of seed data.
/// </summary>
public class SeedDataIntegrityService
{
    private readonly ILogger<SeedDataIntegrityService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SeedDataIntegrityService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public SeedDataIntegrityService(ILogger<SeedDataIntegrityService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Calculates the SHA256 hash of the seed data.
    /// </summary>
    /// <param name="seedData">The seed data as a string.</param>
    /// <returns>The SHA256 hash of the seed data.</returns>
    public string CalculateHash(string seedData)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(seedData);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Verifies the integrity of seed data by comparing its hash with an expected hash.
    /// </summary>
    /// <param name="seedData">The seed data as a string.</param>
    /// <param name="expectedHash">The expected hash.</param>
    /// <returns>True if the hash matches, false otherwise.</returns>
    public bool VerifyIntegrity(string seedData, string expectedHash)
    {
        try
        {
            var actualHash = CalculateHash(seedData);
            var isValid = string.Equals(actualHash, expectedHash, StringComparison.Ordinal);
            
            if (!isValid)
            {
                _logger.LogWarning("Seed data integrity check failed. Expected hash: {ExpectedHash}, Actual hash: {ActualHash}", 
                    expectedHash, actualHash);
            }
            
            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to verify seed data integrity");
            return false;
        }
    }

    /// <summary>
    /// Validates that the seed data is valid JSON.
    /// </summary>
    /// <param name="seedData">The seed data as a string.</param>
    /// <returns>True if the data is valid JSON, false otherwise.</returns>
    public bool ValidateJsonFormat(string seedData)
    {
        try
        {
            using var document = JsonDocument.Parse(seedData);
            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Seed data is not valid JSON");
            return false;
        }
    }
}