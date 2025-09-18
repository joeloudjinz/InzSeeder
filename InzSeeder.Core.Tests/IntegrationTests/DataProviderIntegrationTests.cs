using InzSeeder.Core.Services;
using InzSeeder.Core.Tests.Factories;

namespace InzSeeder.Core.Tests.IntegrationTests;

/// <summary>
/// Integration tests for data provider functionality.
/// </summary>
public class DataProviderIntegrationTests : IAsyncLifetime
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
    /// Tests that the embedded resource data provider can load default seed data.
    /// 
    /// Arrange: Create an EmbeddedResourceSeedDataProvider with the test assembly.
    /// Act: Request seed data for a known seed name.
    /// Assert: Verify that the seed data is loaded correctly from embedded resources.
    /// </summary>
    [Fact]
    public async Task LoadsDefaultSeedDataFromEmbeddedResources()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(DataProviderIntegrationTests).Assembly);

        // Act
        var (content, hash) = await dataProvider.GetSeedDataAsync("Users", CancellationToken.None);

        // Assert
        Assert.NotNull(content);
        Assert.NotNull(hash);
        Assert.NotEmpty(content);
        Assert.NotEmpty(hash);
        
        // Verify the content contains expected user data
        Assert.Contains("john.doe@example.com", content);
        Assert.Contains("Jane Smith", content);
    }

    /// <summary>
    /// Tests that the embedded resource data provider falls back to default data when environment-specific data is not found.
    /// 
    /// Arrange: Set up an environment and create an EmbeddedResourceSeedDataProvider.
    /// Act: Request seed data for a seed name that doesn't have environment-specific variants.
    /// Assert: Verify that the default data is loaded as a fallback.
    /// </summary>
    [Fact]
    public async Task FallsBackToDefaultDataWhenEnvironmentSpecificNotFound()
    {
        // For this test, we need a separate TestDbContextWrapper with a different environment
        var dbContextWrapper = new TestDbContextWrapper();
        dbContextWrapper.SetEnvironment("NonExistentEnvironment");
        await dbContextWrapper.InitializeAsync();
        
        try
        {
            // Arrange
            var context = dbContextWrapper.Context;
            var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(DataProviderIntegrationTests).Assembly);

            // Act
            var (content, hash) = await dataProvider.GetSeedDataAsync("Users", CancellationToken.None);

            // Assert
            Assert.NotNull(content);
            Assert.NotNull(hash);
            Assert.NotEmpty(content);
            Assert.NotEmpty(hash);
            
            // Should fall back to default data
            Assert.Contains("john.doe@example.com", content);
            Assert.Contains("Jane Smith", content);
        }
        finally
        {
            await dbContextWrapper.DisposeAsync();
        }
    }

    /// <summary>
    /// Tests that the embedded resource data provider handles gracefully when requested resources are not found.
    /// 
    /// Arrange: Create an EmbeddedResourceSeedDataProvider and request non-existent seed data.
    /// Act: Request seed data for a non-existent seed name.
    /// Assert: Verify that null values are returned gracefully without exceptions.
    /// </summary>
    [Fact]
    public async Task HandlesMissingResourceGracefully()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(DataProviderIntegrationTests).Assembly);

        // Act
        var (content, hash) = await dataProvider.GetSeedDataAsync("NonExistentSeed", CancellationToken.None);

        // Assert
        Assert.Null(content);
        Assert.Null(hash);
    }
}