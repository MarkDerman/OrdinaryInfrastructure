using DotNet.Testcontainers.Containers;
using Odin.System;
using Respawn;
using System.Data.Common;

namespace Tests.Odin.DDD.Repositories.Database;

/// <summary>
/// To be initialized once for an entire test run.
/// </summary>
public class DatabaseTestContainerFixture : IAsyncDisposable
{
    public DatabaseTestContainerFixture(IDatabaseTestContainerAdapter databaseContainerAdapter)
    {
        ArgumentNullException.ThrowIfNull(databaseContainerAdapter);
        DatabaseTestContainer = databaseContainerAdapter;
        Container = databaseContainerAdapter.BuildContainer();
    }

    private Respawner _respawner = null!;
    public IDatabaseTestContainerAdapter DatabaseTestContainer { get; }
    public IDatabaseContainer Container { get; }

    public string ConnectionString => Container.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        await Container.StartAsync();

        Result migrations = await DatabaseTestContainer.RunMigrationsAsync(ConnectionString, typeof(DatabaseTestContainerFixture).Assembly);
        if (!migrations.IsSuccess)
        {
            throw new Exception($"Database migration scripts failed: {migrations.MessagesToString()}");
        }

        await using DbConnection connection = DatabaseTestContainer.CreateConnection(ConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DatabaseTestContainer.RespawnAdapter,
            SchemasToInclude = // Note, this implicitly excludes all other schemas...
            [
                "dbo"
            ],
            TablesToInclude = [ "BillingPeriod", "BillingEntity", "BillingPeriodProperty", "BillingPeriodTask"],
            WithReseed = true // Sets the seeds of identity PK columns back to their beginning.
        });
    }

    /// <summary>
    /// Deletes everything from data tables.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await using DbConnection connection = DatabaseTestContainer.CreateConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public async ValueTask DisposeAsync()
    {
        await Container.StopAsync();
        await ValueTask.CompletedTask;
    }
}
