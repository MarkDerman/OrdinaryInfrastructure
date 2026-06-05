using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Respawn;
using System.Data.Common;
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
}
