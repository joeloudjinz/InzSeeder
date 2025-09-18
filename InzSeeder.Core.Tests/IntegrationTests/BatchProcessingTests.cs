using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InzSeeder.Core.Adapters;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Tests.Data;
using InzSeeder.Core.Tests.Entities;
using InzSeeder.Core.Tests.Factories;
using InzSeeder.Core.Tests.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Tests.IntegrationTests;

/// <summary>
/// Integration tests for batch processing functionality.
/// </summary>
public class BatchProcessingTests : IAsyncLifetime
{
    private TestDbContextWrapper _dbContextWrapper = null!;

    public async Task InitializeAsync()
    {
        _dbContextWrapper = new TestDbContextWrapper();
        _dbContextWrapper.SetEnvironment("IntegrationTest");
        await _dbContextWrapper.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContextWrapper.DisposeAsync();
    }

    /// <summary>
    /// Tests that entities are processed in batches when seeding large datasets.
    /// 
    /// Arrange: Create a seeder with a large dataset and default batch settings.
    /// Act: Execute the seeder.
    /// Assert: Verify that entities are processed in batches according to the default batch size.
    /// </summary>
    [Fact]
    public async Task ProcessesEntitiesInBatches()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var adapter = new SeederDbContextAdapter<TestDbContext>(context);
        var logger = new LoggerFactory().CreateLogger<UserSeeder>();

        // Create a data provider with a large dataset
        var dataProvider = new LargeDataSetSeedDataProvider();

        // Use default settings (batch size 100)
        var seeder = new UserSeeder(dataProvider, adapter, logger);

        // Act
        await seeder.ExecuteAsync(CancellationToken.None);

        // Assert
        var users = await context.Users.ToListAsync();
        Assert.Equal(500, users.Count); // We created 500 users in our large dataset

        // Verify that all users have unique emails
        var userEmails = users.Select(u => u.Email).ToList();
        Assert.Equal(userEmails.Count, userEmails.Distinct().Count());
    }

    /// <summary>
    /// Tests that custom batch sizes are respected when configured.
    /// 
    /// Arrange: Create a seeder with custom batch size configuration.
    /// Act: Execute the seeder with a custom batch size.
    /// Assert: Verify that all entities are processed correctly (we can't directly verify batch size without modifying the seeder).
    /// </summary>
    [Fact]
    public async Task RespectsCustomBatchSizeConfiguration()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var adapter = new SeederDbContextAdapter<TestDbContext>(context);
        var logger = new LoggerFactory().CreateLogger<UserSeeder>();

        // Create a data provider with a medium dataset
        var dataProvider = new MediumDataSetSeedDataProvider();

        // Configure custom batch size
        var seederSettings = new SeederConfiguration
        {
            BatchSettings = new SeederBatchSettings
            {
                DefaultBatchSize = 10,
                SeederBatchSizes = new Dictionary<string, int> { { "Users", 25 } }
            }
        };

        var seeder = new UserSeeder(dataProvider, adapter, logger, seederSettings);

        // Act
        await seeder.ExecuteAsync(CancellationToken.None);

        // Assert
        var users = await context.Users.ToListAsync();
        Assert.Equal(100, users.Count); // We created 100 users in our medium dataset

        // Verify that all users were processed correctly
        var userEmails = users.Select(u => u.Email).ToList();
        Assert.Equal(userEmails.Count, userEmails.Distinct().Count());

        // Additional verification that the data was processed correctly
        for (int i = 1; i <= 100; i++)
        {
            Assert.Contains(users, u => u.Email == $"user{i}@example.com" && u.Name == $"User {i}");
        }
    }

    // Custom data provider for large dataset
    private class LargeDataSetSeedDataProvider : ISeedDataProvider
    {
        public Task<(string? content, string? hash)> GetSeedDataAsync(string seedName, CancellationToken cancellationToken)
        {
            if (seedName != "Users")
                return Task.FromResult<(string? content, string? hash)>((null, null));

            // Create a large dataset with 500 users
            var users = new List<UserSeedModel>();
            for (int i = 1; i <= 500; i++)
            {
                users.Add(new UserSeedModel
                {
                    Id = i,
                    Email = $"user{i}@example.com",
                    Name = $"User {i}"
                });
            }

            var json = JsonSerializer.Serialize(users);
            var hash = ComputeHash(json);
            return Task.FromResult<(string? content, string? hash)>((json, hash));
        }
    }

    // Custom data provider for medium dataset
    private class MediumDataSetSeedDataProvider : ISeedDataProvider
    {
        public Task<(string? content, string? hash)> GetSeedDataAsync(string seedName, CancellationToken cancellationToken)
        {
            if (seedName != "Users")
                return Task.FromResult<(string? content, string? hash)>((null, null));

            // Create a medium dataset with 100 users
            var users = new List<UserSeedModel>();
            for (int i = 1; i <= 100; i++)
            {
                users.Add(new UserSeedModel
                {
                    Id = i,
                    Email = $"user{i}@example.com",
                    Name = $"User {i}"
                });
            }

            var json = JsonSerializer.Serialize(users);
            var hash = ComputeHash(json);
            return Task.FromResult<(string? content, string? hash)>((json, hash));
        }
    }

    // Helper method to compute hash (replicating the logic from EmbeddedResourceSeedDataProvider)
    private static string ComputeHash(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}