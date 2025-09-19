using InzSeeder.Core.Algorithms;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Core.Tests.Factories;
using InzSeeder.Core.Tests.Seeders;
using Microsoft.EntityFrameworkCore;

namespace InzSeeder.Core.Tests.IntegrationTests;

/// <summary>
/// Integration tests for hash-based change detection functionality.
/// </summary>
public class HashBasedChangeDetectionTests : IAsyncLifetime
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
    /// Tests that the content hash calculation produces consistent results for the same content.
    /// 
    /// Arrange: Use the EmbeddedResourceSeedDataProvider to load the same seed data twice.
    /// Act: Calculate the hash for the same content.
    /// Assert: Verify that the hash is consistent across multiple calls.
    /// </summary>
    [Fact]
    public async Task ProducesConsistentHashForSameContent()
    {
        // Arrange
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(HashBasedChangeDetectionTests).Assembly);

        // Act
        var (content1, hash1) = await dataProvider.GetSeedDataAsync("Users", CancellationToken.None);
        var (content2, hash2) = await dataProvider.GetSeedDataAsync("Users", CancellationToken.None);

        // Assert
        Assert.NotNull(content1);
        Assert.NotNull(content2);
        Assert.NotNull(hash1);
        Assert.NotNull(hash2);
        Assert.Equal(content1, content2);
        Assert.Equal(hash1, hash2);
    }

    /// <summary>
    /// Tests that seed history records are properly persisted after successful seeding.
    /// 
    /// Arrange: Create a seeder and execute it.
    /// Act: Run the seeder to completion.
    /// Assert: Verify that a SeedHistory record was created with the correct hash and timestamp.
    /// </summary>
    [Fact]
    public async Task PersistsSeedHistoryAfterSuccessfulSeeding()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(HashBasedChangeDetectionTests).Assembly);
        var userSeeder = new UserSeeder();
        
        var serviceProvider = _dbContextWrapper.CreateServiceProviderWithSeeders(
            dataProvider, 
            [userSeeder]);

        // Act
        await EnvironmentSeedingOrchestrator.Orchestrate(serviceProvider, CancellationToken.None);

        // Assert
        var seedHistory = await context.Set<SeedHistory>().ToListAsync();
        Assert.Single(seedHistory);
        Assert.Equal("Users", seedHistory[0].SeedIdentifier);
        Assert.NotNull(seedHistory[0].ContentHash);
        Assert.NotEmpty(seedHistory[0].ContentHash);
        Assert.True(seedHistory[0].AppliedDateUtc <= DateTime.UtcNow);
        Assert.True(seedHistory[0].AppliedDateUtc > DateTime.UtcNow.AddMinutes(-1)); // Should be recent
    }

    /// <summary>
    /// Tests that seeding is skipped when the content hash matches existing seed history.
    /// 
    /// Arrange: Run a seeder twice with unchanged data.
    /// Act: Execute the seeder twice in succession.
    /// Assert: Verify that entities were only created once and seeding was skipped on the second run.
    /// </summary>
    [Fact]
    public async Task SkipsSeedingWhenContentHashMatches()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(HashBasedChangeDetectionTests).Assembly);
        var userSeeder = new UserSeeder();
        
        var serviceProvider = _dbContextWrapper.CreateServiceProviderWithSeeders(
            dataProvider, 
            [userSeeder]);

        // Act - Run seeder twice
        await EnvironmentSeedingOrchestrator.Orchestrate(serviceProvider, CancellationToken.None);
        
        // Record initial user count
        var initialUserCount = await context.Users.CountAsync();
        
        // Run again
        await EnvironmentSeedingOrchestrator.Orchestrate(serviceProvider, CancellationToken.None);

        // Assert
        var finalUserCount = await context.Users.CountAsync();
        Assert.Equal(initialUserCount, finalUserCount);
        
        // Check that only one SeedHistory record exists
        var seedHistory = await context.Set<SeedHistory>().ToListAsync();
        Assert.Single(seedHistory);
    }
}