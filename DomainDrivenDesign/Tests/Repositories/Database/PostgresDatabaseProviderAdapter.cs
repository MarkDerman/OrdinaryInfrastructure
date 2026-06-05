using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;
using Tests.Odin.DDD.Repositories.EF;

namespace Tests.Odin.DDD.Repositories.Database;

public class PostgresDatabaseProviderAdapter : DatabaseProviderAdapterBase
{
    public PostgresDatabaseProviderAdapter(string image)
        : base(image, DatabaseProviders.PostgreSQL)
    {
    }

    public PostgresDatabaseProviderAdapter(int version)
        : base(LatestForVersion(version), DatabaseProviders.PostgreSQL)
    {
    }

    public override string MigrationScriptsLocation => ".EF.Postgres.";

    public override IDbAdapter RespawnAdapter => DbAdapter.Postgres;

    public override IDatabaseContainer BuildContainer()
    {
        return new PostgreSqlBuilder(Image).Build();
    }

    public override DbConnection CreateConnection(string connectionString)
    {
        return new NpgsqlConnection(connectionString);
    }

    public override void ConfigureDbContext(
        DbContextOptionsBuilder<TestDatabaseContext> options,
        string connectionString)
    {
        options.UseNpgsql(connectionString);
    }

    public static string LatestForVersion(int version)
    {
        return $"postgres:{version}";
    }
}
