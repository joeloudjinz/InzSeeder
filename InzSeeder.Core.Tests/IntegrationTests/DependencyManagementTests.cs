using InzSeeder.Core.Adapters;
using InzSeeder.Core.Contracts;
using InzSeeder.Core.Services;
using InzSeeder.Core.Tests.Data;
using InzSeeder.Core.Tests.Factories;
using InzSeeder.Core.Tests.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InzSeeder.Core.Tests.IntegrationTests;

/// <summary>
/// Integration tests for seeder dependency management functionality.
/// </summary>
public class DependencyManagementTests : IAsyncLifetime
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
    /// Tests that seeders with dependencies are executed in the correct order.
    /// UserProfiles depend on Users, so Users must be seeded first.
    /// 
    /// Arrange: Create UserSeeder and UserProfileSeeder with a dependency relationship.
    /// Act: Use the seeder sorter to arrange them in correct dependency order and execute them.
    /// Assert: Verify that all seeders executed successfully and dependent data was created correctly.
    /// </summary>
    [Fact]
    public async Task ExecutesSeedersInDependencyOrder()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var adapter = new SeederDbContextAdapter<TestDbContext>(context);
        var loggerFactory = new LoggerFactory();
        var dataProvider = new EmbeddedResourceSeedDataProvider(typeof(DependencyManagementTests).Assembly);

        // Create seeders with proper dependencies
        var userSeeder = new UserSeeder(dataProvider, adapter, loggerFactory.CreateLogger<UserSeeder>());
        var userProfileSeeder = new UserProfileSeeder(dataProvider, adapter, loggerFactory.CreateLogger<UserProfileSeeder>());

        // Create a list of seeders in wrong order to test dependency sorting
        var seeders = new List<IEntitySeeder> { userProfileSeeder, userSeeder };

        // Sort seeders using the SeederSorter
        var sortedSeeders = SeederSorter.Sort(seeders);

        // Act
        foreach (var seeder in sortedSeeders)
        {
            await seeder.ExecuteAsync(CancellationToken.None);
        }

        // Assert
        // Check that users were created
        var users = await context.Users.ToListAsync();
        Assert.Equal(2, users.Count);
        
        // Check that user profiles were created
        var userProfiles = await context.UserProfiles.ToListAsync();
        Assert.Equal(2, userProfiles.Count);
        
        // Verify the foreign key relationships
        var john = users.FirstOrDefault(u => u.Email == "john.doe@example.com");
        var johnProfile = userProfiles.FirstOrDefault(up => up.UserId == john?.Id);
        Assert.NotNull(johnProfile);
        Assert.Equal("John Doe's bio", johnProfile.Bio);
        
        var jane = users.FirstOrDefault(u => u.Email == "jane.smith@example.com");
        var janeProfile = userProfiles.FirstOrDefault(up => up.UserId == jane?.Id);
        Assert.NotNull(janeProfile);
        Assert.Equal("Jane Smith's bio", janeProfile.Bio);
    }

    /// <summary>
    /// Tests that circular dependencies between seeders are detected and cause an exception.
    /// 
    /// Arrange: Create seeders with circular dependencies.
    /// Act: Attempt to sort the seeders using the seeder sorter.
    /// Assert: Verify that an InvalidOperationException is thrown.
    /// </summary>
    [Fact]
    public void DetectsCircularDependencies()
    {
        // Arrange
        // Create circular dependency scenario
        var circularSeeder1 = new CircularTestSeeder1();
        var circularSeeder2 = new CircularTestSeeder2();

        var seeders = new List<IEntitySeeder> { circularSeeder1, circularSeeder2 };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => SeederSorter.Sort(seeders));
        Assert.Contains("circular", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    // Test seeders with circular dependencies
    private class CircularTestSeeder1 : IEntitySeeder
    {
        public string SeedName => "CircularTestSeeder1";

        public IEnumerable<Type> Dependencies => [typeof(CircularTestSeeder2)];

        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private class CircularTestSeeder2 : IEntitySeeder
    {
        public string SeedName => "CircularTestSeeder2";

        public IEnumerable<Type> Dependencies => [typeof(CircularTestSeeder1)];

        public Task ExecuteAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}