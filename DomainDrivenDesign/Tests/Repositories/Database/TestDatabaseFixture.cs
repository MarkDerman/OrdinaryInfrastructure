using DbUp.Engine.Transactions;
using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;
using Odin.Database;
using Odin.System;
using Respawn;

namespace Tests.Odin.DDD.Repositories.Database;

/// <summary>
/// To be initialized once for an entire test run.
/// </summary>
public class TestDatabaseFixture : IAsyncDisposable
{
    public TestDatabaseFixture(IDatabaseContainerBuilder containerBuilder, IDbAdapter respawnAdaptor)
    {
        ArgumentNullException.ThrowIfNull(containerBuilder);
        ArgumentNullException.ThrowIfNull(respawnAdaptor);
        Image = containerBuilder.Image;
        Server = containerBuilder.DatabaseProvider;
        RespawnAdaptor = respawnAdaptor;
        Container = containerBuilder.Build();
    }

    private Respawner _respawner = null!;
    public IDatabaseContainer Container { get;  }
    public string Image { get;  }
    public string Server { get;  }
    public IDbAdapter RespawnAdaptor { get; }
    
    public string ConnectionString => Container.GetConnectionString();
    
    public async ValueTask InitializeAsync()
    {
        await Container.StartAsync();
        
        Result migrations = RunMigrations(ConnectionString);
        if (!migrations.IsSuccess)
            throw new Exception($"Database migration scripts failed: {migrations.MessagesToString()}");

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = RespawnAdaptor,
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
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public async ValueTask DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync();
    }
    
    public static Result RunMigrations(string connectionString)
    {
        ResultValue<SqlScriptsRunner> migrationsRunner =
            SqlScriptsRunner.CreateFromConnectionString(connectionString, typeof(TestDatabaseFixture).Assembly);
        if (!migrationsRunner.IsSuccess) return Result.Failure(migrationsRunner.Messages);

        SqlScriptsRunner runner = migrationsRunner.Value;
        runner.EnsureDatabaseCreated = true;
        runner.JournalMode = JournalModeEnum.RunOnlyScriptsNotRunBefore;
        runner.JournalToTableName = "DatabaseMigrations";
        runner.ScriptsLocationType = ScriptsLocationTypeEnum.EmbeddedResourcePath;
        runner.ScriptsLocation = ".EF.SqlServer.";
        runner.TransactionHandling = TransactionMode.TransactionPerScript;
        return runner.Run();
    }
}