using DbUp.Engine.Transactions;
using DotNet.Testcontainers.Containers;
using Odin.Database;
using Odin.System;
using Respawn;
using System.Data.Common;

namespace Tests.Odin.DDD.Repositories.Database;

/// <summary>
/// To be initialized once for an entire test run.
/// </summary>
public class TestDatabaseFixture : IAsyncDisposable
{
    public TestDatabaseFixture(IDatabaseProviderAdapter databaseProvider)
    {
        ArgumentNullException.ThrowIfNull(databaseProvider);
        DatabaseProvider = databaseProvider;
        Image = databaseProvider.Image;
        Server = databaseProvider.DatabaseProvider;
        Container = databaseProvider.BuildContainer();
    }

    private Respawner _respawner = null!;
    public IDatabaseProviderAdapter DatabaseProvider { get; }
    public IDatabaseContainer Container { get;  }
    public string Image { get;  }
    public string Server { get;  }
    
    public string ConnectionString => Container.GetConnectionString();
    
    public async ValueTask InitializeAsync()
    {
        await Container.StartAsync();
        
        Result migrations = RunMigrations(ConnectionString, DatabaseProvider.MigrationScriptsLocation);
        if (!migrations.IsSuccess)
            throw new Exception($"Database migration scripts failed: {migrations.MessagesToString()}");

        await using DbConnection connection = DatabaseProvider.CreateConnection(ConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DatabaseProvider.RespawnAdapter,
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
        await using DbConnection connection = DatabaseProvider.CreateConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public async ValueTask DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync();
    }
    
    public static Result RunMigrations(string connectionString, string scriptsLocation)
    {
        ResultValue<SqlScriptsRunner> migrationsRunner =
            SqlScriptsRunner.CreateFromConnectionString(connectionString, typeof(TestDatabaseFixture).Assembly);
        if (!migrationsRunner.IsSuccess) return Result.Failure(migrationsRunner.Messages);

        SqlScriptsRunner runner = migrationsRunner.Value;
        runner.EnsureDatabaseCreated = true;
        runner.JournalMode = JournalModeEnum.RunOnlyScriptsNotRunBefore;
        runner.JournalToTableName = "DatabaseMigrations";
        runner.ScriptsLocationType = ScriptsLocationTypeEnum.EmbeddedResourcePath;
        runner.ScriptsLocation = scriptsLocation;
        runner.TransactionHandling = TransactionMode.TransactionPerScript;
        return runner.Run();
    }
}
