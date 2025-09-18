using InzSeeder.Core.Adapters;
using InzSeeder.Core.Tests.Data;
using InzSeeder.Core.Tests.Entities;
using InzSeeder.Core.Tests.Factories;
using Microsoft.EntityFrameworkCore;

namespace InzSeeder.Core.Tests.IntegrationTests;

/// <summary>
/// Integration tests for database context adapter functionality.
/// </summary>
public class DatabaseContextIntegrationTests : IAsyncLifetime
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
    /// Tests that the DbContext adapter correctly implements the ISeederDbContext interface.
    /// 
    /// Arrange: Create a TestDbContext and wrap it with SeederDbContextAdapter.
    /// Act: Use the adapter to perform various DbContext operations.
    /// Assert: Verify that all operations work correctly through the adapter.
    /// </summary>
    [Fact]
    public async Task DbContextAdapterFunctionsCorrectly()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var adapter = new SeederDbContextAdapter<TestDbContext>(context);

        // Act & Assert
        // Test that we can get a DbSet through the adapter
        var userDbSet = adapter.Set<User>();
        Assert.NotNull(userDbSet);

        // Test that we can add entities through the adapter
        var user = new User
        {
            Email = "test@example.com",
            Name = "Test User"
        };

        await adapter.Set<User>().AddAsync(user);
        var savedChanges = await adapter.SaveChangesAsync();

        // Assert
        Assert.Equal(1, savedChanges);
        Assert.True(user.Id > 0); // Should have been assigned an ID

        // Test that we can query through the adapter
        var users = await adapter.Set<User>().ToListAsync();
        Assert.Single(users);
        Assert.Equal("test@example.com", users[0].Email);
    }

    /// <summary>
    /// Tests that transaction handling works correctly with the DbContext adapter.
    /// 
    /// Arrange: Create a TestDbContext and wrap it with SeederDbContextAdapter.
    /// Act: Begin a transaction, make changes, then roll back the transaction.
    /// Assert: Verify that changes are not persisted after rollback.
    /// </summary>
    [Fact]
    public async Task HandlesTransactionsCorrectly()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var adapter = new SeederDbContextAdapter<TestDbContext>(context);

        // Act
        using var transaction = await adapter.Database.BeginTransactionAsync();
        
        // Add a user within the transaction
        var user = new User
        {
            Email = "transactiontest@example.com",
            Name = "Transaction Test User"
        };
        
        await adapter.Set<User>().AddAsync(user);
        await adapter.SaveChangesAsync();

        // Verify user exists within the transaction
        var usersInTransaction = await adapter.Set<User>().ToListAsync();
        Assert.Single(usersInTransaction);
        Assert.Equal("transactiontest@example.com", usersInTransaction[0].Email);

        // Rollback the transaction
        await transaction.RollbackAsync();

        // Assert - user should not exist after rollback
        var usersAfterRollback = await adapter.Set<User>().ToListAsync();
        Assert.Empty(usersAfterRollback);
    }

    /// <summary>
    /// Tests that the DbContext adapter correctly exposes the Database and ChangeTracker properties.
    /// 
    /// Arrange: Create a TestDbContext and wrap it with SeederDbContextAdapter.
    /// Act: Access the Database and ChangeTracker properties.
    /// Assert: Verify that the properties are accessible and functional.
    /// </summary>
    [Fact]
    public void ExposesDatabaseAndChangeTrackerProperties()
    {
        // Arrange
        var context = _dbContextWrapper.Context;
        var adapter = new SeederDbContextAdapter<TestDbContext>(context);

        // Act & Assert
        // Test Database property
        Assert.NotNull(adapter.Database);
        // We can't easily check if it's in-memory without additional dependencies
        // but we can verify it's not null

        // Test ChangeTracker property
        Assert.NotNull(adapter.ChangeTracker);
        Assert.Equal(context.ChangeTracker, adapter.ChangeTracker); // Should be the same instance
    }
}