using DbUp.Engine.Transactions;
using Odin.Database;
using Odin.System;

namespace Tests.Odin.DDD.Repositories.EF;

public static class MigrationScriptsRunner
{
    public static Result RunMigrations(string connectionString)
    {
        ResultValue<SqlScriptsRunner> migrationsRunner =
            SqlScriptsRunner.CreateFromConnectionString(connectionString, typeof(MigrationScriptsRunner).Assembly);
        if (!migrationsRunner.IsSuccess) return Result.Failure(migrationsRunner.Messages);

        SqlScriptsRunner runner = migrationsRunner.Value;
        runner.EnsureDatabaseCreated = true;
        runner.JournalMode = JournalModeEnum.RunOnlyScriptsNotRunBefore;
        runner.JournalToTableName = "DatabaseMigrations";
        runner.ScriptsLocationType = ScriptsLocationTypeEnum.EmbeddedResourcePath;
        runner.ScriptsLocation = ".DatabaseMigrations.RunOnce.";
        runner.TransactionHandling = TransactionMode.TransactionPerScript;
        Result migrationsOutcome = runner.Run();

        ResultValue<SqlScriptsRunner> alwaysRunScriptsRunner =
            SqlScriptsRunner.CreateFromConnectionString(connectionString, typeof(MigrationScriptsRunner).Assembly);
        if (!alwaysRunScriptsRunner.IsSuccess) return Result.Failure(alwaysRunScriptsRunner.Messages);

        SqlScriptsRunner runner2 = alwaysRunScriptsRunner.Value;
        runner2.EnsureDatabaseCreated = false;
        runner2.JournalMode = JournalModeEnum.AlwaysRunAllScripts;
        runner2.ScriptsLocationType = ScriptsLocationTypeEnum.EmbeddedResourcePath;
        runner2.ScriptsLocation = ".DatabaseMigrations.AlwaysRun.";
        runner2.TransactionHandling = TransactionMode.TransactionPerScript;
        Result viewsRecreateOutcome = runner2.Run();

        return Result.Combine(migrationsOutcome, viewsRecreateOutcome);
    }

}