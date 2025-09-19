using InzSeeder.Core.Attributes;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Extensions;
using InzSeeder.Core.Tests.Factories;
using InzSeeder.Core.Utilities;

namespace InzSeeder.Core.Tests.IntegrationTests;

/// <summary>
/// Integration tests for environment awareness functionality.
/// </summary>
public class EnvironmentAwarenessIntegrationTests : IAsyncLifetime
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
    /// Tests that environment detection works correctly from command line arguments.
    /// 
    /// Arrange: Set environment using command line argument simulation.
    /// Act: Call EnvironmentUtility.DetermineEnvironment with a command line value.
    /// Assert: Verify that the command line environment is used.
    /// </summary>
    [Fact]
    public void DetectsEnvironmentFromCommandLine()
    {
        // Arrange
        const string commandLineEnvironment = "Staging";

        // Act
        var environment = EnvironmentUtility.DetermineEnvironment(commandLineEnvironment);

        // Assert
        Assert.Equal(commandLineEnvironment, environment);
    }

    /// <summary>
    /// Tests that environment detection works correctly from environment variables.
    /// 
    /// Arrange: Set SEEDING_ENVIRONMENT environment variable.
    /// Act: Call EnvironmentUtility.DetermineEnvironment without command line value.
    /// Assert: Verify that the environment variable value is used.
    /// </summary>
    [Fact]
    public void DetectsEnvironmentFromEnvironmentVariable()
    {
        // Arrange
        const string environmentVariableValue = "Production";
        Environment.SetEnvironmentVariable("SEEDING_ENVIRONMENT", environmentVariableValue);

        // Act
        var environment = EnvironmentUtility.DetermineEnvironment();

        // Assert
        Assert.Equal(environmentVariableValue, environment);

        // Cleanup
        Environment.SetEnvironmentVariable("SEEDING_ENVIRONMENT", null);
    }

    /// <summary>
    /// Tests that environment detection falls back to ASPNETCORE_ENVIRONMENT when other methods don't provide a value.
    /// 
    /// Arrange: Set ASPNETCORE_ENVIRONMENT environment variable.
    /// Act: Call EnvironmentUtility.DetermineEnvironment without other values.
    /// Assert: Verify that the ASPNETCORE_ENVIRONMENT value is used.
    /// </summary>
    [Fact]
    public void DetectsEnvironmentFromAspNetCoreEnvironmentVariable()
    {
        // Arrange
        const string aspNetCoreEnvironmentValue = "Development";
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", aspNetCoreEnvironmentValue);

        // Act
        var environment = EnvironmentUtility.DetermineEnvironment();

        // Assert
        Assert.Equal(aspNetCoreEnvironmentValue, environment);

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    /// <summary>
    /// Tests that the EnvironmentCompatibilityAttribute correctly identifies whether a seeder is allowed in a specific environment.
    /// 
    /// Arrange: Create seeders with different environment compatibility attributes.
    /// Act: Check if seeders are allowed in various environments.
    /// Assert: Verify that the compatibility filtering works correctly.
    /// </summary>
    [Fact]
    public void EnvironmentCompatibilityAttributeFiltersSeedersCorrectly()
    {
        // Arrange
        var productionSafeSeeder = new ProductionSafeTestSeeder();
        var developmentOnlySeeder = new DevelopmentOnlyTestSeeder();

        // Act & Assert
        // Test in Production environment
        Assert.True(productionSafeSeeder.IsAllowedInEnvironment("Production"));
        Assert.False(developmentOnlySeeder.IsAllowedInEnvironment("Production"));

        // Test in Development environment
        Assert.False(productionSafeSeeder.IsAllowedInEnvironment("Development")); // Only allowed in Production
        Assert.True(developmentOnlySeeder.IsAllowedInEnvironment("Development"));
    }

    /// <summary>
    /// Tests that seeders marked as production safe are correctly identified.
    /// 
    /// Arrange: Create seeders with and without ProductionSafe attribute.
    /// Act: Check if seeders are identified as production safe.
    /// Assert: Verify that the identification works correctly.
    /// </summary>
    [Fact]
    public void IdentifiesProductionSafeSeeders()
    {
        // Arrange
        var productionSafeSeeder = new ProductionSafeTestSeeder();
        var regularSeeder = new RegularTestSeeder();

        // Act & Assert
        Assert.True(productionSafeSeeder.IsProductionSafe());
        Assert.False(regularSeeder.IsProductionSafe());
    }

    // Test seeder classes
    [EnvironmentCompatibility(true, "Production")]
    private class ProductionSafeTestSeeder : IBaseEntityDataSeeder
    {
        public string SeedName => "ProductionSafeTestSeeder";

        public IEnumerable<Type> Dependencies => [];
    }

    [EnvironmentCompatibility(false, "Development")]
    private class DevelopmentOnlyTestSeeder : IBaseEntityDataSeeder
    {
        public string SeedName => "DevelopmentOnlyTestSeeder";

        public IEnumerable<Type> Dependencies => [];
    }

    private class RegularTestSeeder : IBaseEntityDataSeeder
    {
        public string SeedName => "RegularTestSeeder";

        public IEnumerable<Type> Dependencies => [];
    }
}