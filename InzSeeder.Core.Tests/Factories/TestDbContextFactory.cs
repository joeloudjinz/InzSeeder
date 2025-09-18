using InzSeeder.Core.Tests.Data;
using InzSeeder.Core.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

// Life savor => https://stackoverflow.com/questions/1408175/execute-unit-tests-serially-rather-than-in-parallel
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace InzSeeder.Core.Tests.Factories;

public class TestDbContextWrapper : IAsyncLifetime
{
    private SqliteConnection? _connection;
    private TestDbContext? _context;

    public TestDbContext Context => _context ?? throw new InvalidOperationException("Context has not been initialized. Call InitializeAsync first.");

    public async Task InitializeAsync()
    {
        var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
        _connection = new SqliteConnection(connectionStringBuilder.ToString());
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new TestDbContext(options);
        await _context.Database.OpenConnectionAsync();
        await _context.Database.EnsureCreatedAsync();
    }

    public void SetEnvironment(string? env)
    {
        EnvironmentUtility.ResetForTesting();
        EnvironmentUtility.DetermineEnvironment(env);
    }

    public async Task DisposeAsync()
    {
        EnvironmentUtility.ResetForTesting();

        if (_context != null)
        {
            await _context.DisposeAsync();
        }

        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
}

// public static class TestDbContextFactory
// {
//     public static TestDbContext CreateContext(string? environment = "IntegrationTest")
//     {
//         // Reset environment for testing to ensure isolation
//         EnvironmentUtility.ResetForTesting();
//
//         // Set up environment for tests
//         var result = EnvironmentUtility.DetermineEnvironment(environment);
//         Console.WriteLine($"[DEBUG] TestDbContextFactory.CreateContext called with environment='{environment}', DetermineEnvironment returned '{result}'");
//
//         var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
//         var connection = new SqliteConnection(connectionStringBuilder.ToString());
//         var options = new DbContextOptionsBuilder<TestDbContext>()
//             .UseSqlite(connection)
//             .Options;
//
//         var context = new TestDbContext(options);
//         context.Database.OpenConnection();
//         context.Database.EnsureCreated();
//         return context;
//     }
// }