using DbUp.Engine.Transactions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Odin.Data;
using Odin.System;
using Respawn;

namespace Tests.Odin.DDD.DB;

/// <summary>
/// For XUnit shared DatabaseFixture
/// </summary>
[CollectionDefinition(nameof(DatabaseCollection))]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

/// <summary>
/// Initializes once, rolling back and migrating the target AVT database.
/// ResetDatabaseAsync() is used in IntegrationTestBase
/// to reset all application data tables in the Billing schema only, using Respawn.
/// </summary>
public class DatabaseFixture : IAsyncLifetime
{
    private Respawner _respawner = null!;

    public DatabaseFixture()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json").Build();
        ConnectionString = config.GetConnectionString("Default")!;
    }

    public string ConnectionString { get; } 

    public async ValueTask InitializeAsync()
    {
        Result rollback = RollbackDatabaseSchema();
        if (!rollback.IsSuccess) 
            throw new Exception($"Database rollback scripts failed: {rollback.MessagesToString()}");

        Result migrations = MigrateDatabaseSchema();
        if (!migrations.IsSuccess)
            throw new Exception($"Database migration scripts failed: {migrations.MessagesToString()}");

        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.SqlServer,
            SchemasToInclude = // Note, this implicitly excludes all other schemas...
            [
                "dbo"
            ],
            TablesToInclude = [ // Note, this implicitly excludes all other tables...
                "BillingEntity", "BillingPeriod"
            ],
            WithReseed = true // Sets the seeds of 'Id' identity columns back to 1.
        });
    }
    
    internal Result RollbackDatabaseSchema()
    {
        ResultValue<SqlScriptsRunner> runnerCreate = 
            SqlScriptsRunner.CreateFromConnectionString(ConnectionString, typeof(DatabaseFixture).Assembly);
        if (!runnerCreate.IsSuccess) return Result.Failure(runnerCreate.Messages);

        SqlScriptsRunner runner = runnerCreate.Value;
        runner.EnsureDatabaseCreated = false;
        runner.JournalMode = JournalModeEnum.AlwaysRunAllScripts;
        runner.ScriptsLocationType = ScriptsLocationTypeEnum.EmbeddedResourcePath;
        runner.ScriptsLocation = ".DB.SqlServer.Rollback.";
        runner.TransactionHandling = TransactionMode.TransactionPerScript;
        return runner.Run();
    }
    
    internal Result MigrateDatabaseSchema()
    {
        ResultValue<SqlScriptsRunner> runnerCreate = 
            SqlScriptsRunner.CreateFromConnectionString(ConnectionString, typeof(DatabaseFixture).Assembly);
        if (!runnerCreate.IsSuccess) return Result.Failure(runnerCreate.Messages);
        
        SqlScriptsRunner runner = runnerCreate.Value;
        runner.EnsureDatabaseCreated = true;
        runner.JournalMode = JournalModeEnum.RunOnlyScriptsNotRunBefore;
        runner.JournalToTableName = "DatabaseMigrations";
        runner.ScriptsLocationType = ScriptsLocationTypeEnum.EmbeddedResourcePath;
        runner.ScriptsLocation = ".DB.SqlServer.Migrate.";
        runner.TransactionHandling = TransactionMode.TransactionPerScript;
        return runner.Run();
    }

    /// <summary>
    /// Deletes everything from 'Billing' schema application data tables.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await using var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}