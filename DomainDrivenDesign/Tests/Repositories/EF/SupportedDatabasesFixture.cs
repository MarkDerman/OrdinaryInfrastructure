using DbUp.Engine.Transactions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Odin.Database;
using Odin.System;
using Respawn;

namespace Tests.Odin.DDD.Repositories.EF;

/// <summary>
/// For XUnit shared SupportedDatabasesFixture per test run
/// </summary>
[CollectionDefinition(nameof(SupportedDatabasesCollection))]
public class SupportedDatabasesCollection : ICollectionFixture<SupportedDatabasesFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

/// <summary>
/// Initializes once, rolling back and migrating the target AVT database.
/// ResetDatabaseAsync() is used in IntegrationTestBase
/// </summary>
public class SupportedDatabasesFixture : IAsyncLifetime
{
    private Respawner _respawner = null!;

    public SupportedDatabasesFixture()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json").Build();
        ConnectionString = config.GetConnectionString("Default")!;
    }

    public string ConnectionString { get; }

    public async ValueTask InitializeAsync()
    {
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
            WithReseed = true // Sets the seeds of identity PK columns back to their beginning.
        });
    }

    internal Result MigrateDatabaseSchema()
    {
        ResultValue<SqlScriptsRunner> runnerCreate =
            SqlScriptsRunner.CreateFromConnectionString(ConnectionString, typeof(SupportedDatabasesFixture).Assembly);
        if (!runnerCreate.IsSuccess) return Result.Failure(runnerCreate.Messages);

        SqlScriptsRunner runner = runnerCreate.Value;
        runner.EnsureDatabaseCreated = true;
        runner.JournalMode = JournalModeEnum.RunOnlyScriptsNotRunBefore;
        runner.JournalToTableName = "DatabaseMigrations";
        runner.ScriptsLocationType = ScriptsLocationTypeEnum.EmbeddedResourcePath;
        runner.ScriptsLocation = "Repositories.EF.SqlServer.";
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
