using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Odin.System;
using Respawn;
using System.Data.Common;
using System.Reflection;
using Tests.Odin.DDD.Repositories.EF;

namespace Tests.Odin.DDD.Repositories.Database;

/// <inheritdoc />
public abstract class DatabaseProviderAdapterBase : IDatabaseProviderAdapter
{
    protected DatabaseProviderAdapterBase(string image, string databaseProvider)
    {
        Image = image;
        DatabaseProvider = databaseProvider;
    }

    /// <inheritdoc />
    public string Image { get; }

    /// <inheritdoc />
    public string DatabaseProvider { get; }

    /// <inheritdoc />
    public abstract string MigrationScriptsLocation { get; }

    /// <inheritdoc />
    public abstract IDbAdapter RespawnAdapter { get; }

    /// <inheritdoc />
    public abstract IDatabaseContainer BuildContainer();

    /// <inheritdoc />
    public abstract DbConnection CreateConnection(string connectionString);

    /// <inheritdoc />
    public abstract void ConfigureDbContext(
        DbContextOptionsBuilder<TestDatabaseContext> options,
        string connectionString);

    /// <inheritdoc />
    public virtual async Task<Result> RunMigrationsAsync(string connectionString, Assembly assemblyWithEmbeddedScripts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(assemblyWithEmbeddedScripts);

        string[] scriptNames = assemblyWithEmbeddedScripts.GetManifestResourceNames()
            .Where(resourceName => resourceName.Contains(MigrationScriptsLocation, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (scriptNames.Length == 0)
        {
            return Result.Failure($"No embedded migration scripts were found for '{MigrationScriptsLocation}'.");
        }

        try
        {
            await using DbConnection connection = CreateConnection(connectionString);
            await connection.OpenAsync();

            foreach (string scriptName in scriptNames)
            {
                await using Stream? stream = assemblyWithEmbeddedScripts.GetManifestResourceStream(scriptName);
                if (stream == null)
                {
                    return Result.Failure($"Unable to open embedded migration script '{scriptName}'.");
                }

                using StreamReader reader = new StreamReader(stream);
                string script = await reader.ReadToEndAsync();

                foreach (string commandText in SplitScript(script))
                {
                    if (string.IsNullOrWhiteSpace(commandText))
                    {
                        continue;
                    }

                    await using DbCommand command = connection.CreateCommand();
                    command.CommandText = commandText;
                    await command.ExecuteNonQueryAsync();
                }
            }

            return Result.Success($"{scriptNames.Length} migration scripts executed successfully.");
        }
        catch (Exception err)
        {
            return Result.Failure($"Database migration scripts failed for {DatabaseProvider}: {err.Message}");
        }
    }

    protected virtual IEnumerable<string> SplitScript(string script)
    {
        yield return script;
    }
}
