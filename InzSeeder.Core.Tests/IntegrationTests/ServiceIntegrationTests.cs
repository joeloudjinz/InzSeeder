using InzSeeder.Core.Algorithms;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Models;
using InzSeeder.Core.Services;
using InzSeeder.Core.Tests.Factories;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Tests.IntegrationTests;

/// <summary>
/// Integration tests for service functionality.
/// </summary>
public class ServiceIntegrationTests : IAsyncLifetime
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
    /// Tests that the seeder sorting service correctly performs topological sorting of seeders with dependencies.
    /// 
    /// Arrange: Create seeders with complex dependency relationships.
    /// Act: Use the SeederSorter to sort the seeders.
    /// Assert: Verify that the seeders are sorted in the correct dependency order.
    /// </summary>
    [Fact]
    public void SortsSeedersInCorrectDependencyOrder()
    {
        // Arrange
        var seederA = new TestSeederA(); // No dependencies
        var seederB = new TestSeederB(); // Depends on A
        var seederC = new TestSeederC(); // Depends on A
        var seederD = new TestSeederD(); // Depends on B and C

        var seeders = new List<IBaseEntityDataSeeder> { seederD, seederB, seederC, seederA }; // Deliberately out of order

        // Act
        var sortedSeeders = SeederSorter.Sort(seeders).ToList();

        // Assert
        Assert.Equal(4, sortedSeeders.Count);

        // A should come first (no dependencies)
        Assert.Equal("TestSeederA", sortedSeeders[0].SeedName);

        // B and C should come next (depend on A, but independent of each other)
        var bIndex = sortedSeeders.FindIndex(s => s.SeedName == "TestSeederB");
        var cIndex = sortedSeeders.FindIndex(s => s.SeedName == "TestSeederC");
        var aIndex = sortedSeeders.FindIndex(s => s.SeedName == "TestSeederA");

        Assert.True(bIndex > aIndex);
        Assert.True(cIndex > aIndex);

        // D should come last (depends on both B and C)
        var dIndex = sortedSeeders.FindIndex(s => s.SeedName == "TestSeederD");
        Assert.True(dIndex > bIndex);
        Assert.True(dIndex > cIndex);
    }

    /// <summary>
    /// Tests that the profile validation service correctly validates valid configurations.
    /// 
    /// Arrange: Create a seeding profile with valid configuration.
    /// Act: Use the SeedingProfileValidationService to validate the profile.
    /// Assert: Verify that the validation passes for valid configurations.
    /// </summary>
    [Fact]
    public void ValidatesValidSeedingProfiles()
    {
        // Arrange
        var seederA = new TestSeederA();
        var seederB = new TestSeederB();
        var seeders = new List<IBaseEntityDataSeeder> { seederA, seederB };

        var loggerFactory = new LoggerFactory();
        var validationService = new SeedingProfileValidationService(seeders, loggerFactory.CreateLogger<SeedingProfileValidationService>());

        var seederConfiguration = new SeederConfiguration
        {
            Profile = new SeedingProfile
            {
                EnabledSeeders = ["TestSeederA", "TestSeederB"]
            }
        };

        // Act
        var context = _dbContextWrapper.Context; // This will set the environment
        var isValid = validationService.ValidateSettings(seederConfiguration);

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Tests that the profile validation service correctly identifies invalid seeder references.
    /// 
    /// Arrange: Create a seeding profile with references to non-existent seeders.
    /// Act: Use the SeedingProfileValidationService to validate the profile.
    /// Assert: Verify that the validation fails for invalid seeder references.
    /// </summary>
    [Fact]
    public void DetectsInvalidSeederReferencesInProfiles()
    {
        // Arrange
        var seederA = new TestSeederA();
        var seeders = new List<IBaseEntityDataSeeder> { seederA };

        var loggerFactory = new LoggerFactory();
        var validationService = new SeedingProfileValidationService(seeders, loggerFactory.CreateLogger<SeedingProfileValidationService>());

        var seederConfiguration = new SeederConfiguration
        {
            Profile = new SeedingProfile
            {
                EnabledSeeders = ["TestSeederA", "NonExistentSeeder"] // NonExistentSeeder doesn't exist
            }
        };

        // Act
        var context = _dbContextWrapper.Context; // This will set the environment
        var isValid = validationService.ValidateSettings(seederConfiguration);

        // Assert
        Assert.False(isValid);
    }

    // Test seeders with dependencies
    private class TestSeederA : IBaseEntityDataSeeder
    {
        public string SeedName => "TestSeederA";

        public IEnumerable<Type> Dependencies => [];
    }

    private class TestSeederB : IBaseEntityDataSeeder
    {
        public string SeedName => "TestSeederB";

        public IEnumerable<Type> Dependencies => [typeof(TestSeederA)];
    }

    private class TestSeederC : IBaseEntityDataSeeder
    {
        public string SeedName => "TestSeederC";

        public IEnumerable<Type> Dependencies => [typeof(TestSeederA)];
    }

    private class TestSeederD : IBaseEntityDataSeeder
    {
        public string SeedName => "TestSeederD";

        public IEnumerable<Type> Dependencies => [typeof(TestSeederB), typeof(TestSeederC)];
    }
}