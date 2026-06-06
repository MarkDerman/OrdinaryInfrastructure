using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Microsoft.EntityFrameworkCore;
using Odin.System;
using Respawn;
using System.Data.Common;
using System.Reflection;
using Tests.Odin.DDD.Repositories.EF;

namespace Tests.Odin.DDD.Repositories.Database;

/// <inheritdoc />
public abstract class DatabaseTestContainerAdapterBase<TContainerBuilder, TContainer, TConfiguration>
    : IDatabaseTestContainerAdapter
    where TContainerBuilder : ContainerBuilder<TContainerBuilder, TContainer, TConfiguration>
    where TContainer : IDatabaseContainer
    where TConfiguration : IContainerConfiguration
{
    protected DatabaseTestContainerAdapterBase(string containerNamePrefix, string image, string databaseProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(containerNamePrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(image);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseProvider);
        Image = new DockerImage(image);
        DatabaseProvider = databaseProvider;
        ContainerName = $"{containerNamePrefix}-{DatabaseProvider}.{Image.Tag}";
    }

    /// <inheritdoc />
    public DockerImage Image { get; }

    /// <inheritdoc />
    public string DatabaseProvider { get; }

    /// <inheritdoc />
    public string ContainerName { get; }

    /// <inheritdoc />
    public abstract string MigrationScriptsLocation { get; }

    /// <inheritdoc />
    public abstract IDbAdapter RespawnAdapter { get; }

    /// <inheritdoc />
    public IDatabaseContainer BuildContainer()
    {
        return CreateContainerBuilder()
            .WithName(ContainerName)
            .WithReuse(true)
            .WithCleanUp(false)
            .Build();
    }

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

            if (await ApplicationSchemaExistsAsync(connection))
            {
                return Result.Success($"{DatabaseProvider} test database schema already exists.");
            }

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

    private static async Task<bool> ApplicationSchemaExistsAsync(DbConnection connection)
    {
        await using DbCommand command = connection.CreateCommand();
        command.CommandText = """
            select count(*)
            from information_schema.tables
            where table_schema = 'dbo'
              and table_name = 'BillingEntity'
            """;

        object? result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0;
    }

    protected abstract TContainerBuilder CreateContainerBuilder();
}
