using Microsoft.EntityFrameworkCore;
using Npgsql;
using Respawn;
using System.Data.Common;
using Testcontainers.PostgreSql;
using Tests.Odin.DDD.Repositories.EF;

namespace Tests.Odin.DDD.Repositories.Database;

public class PostgresTestContainerAdapter : DatabaseTestContainerAdapterBase
    <PostgreSqlBuilder, PostgreSqlContainer, PostgreSqlConfiguration>
{
    public PostgresTestContainerAdapter(string containerNamePrefix, string image)
        : base(containerNamePrefix,image, DatabaseProviders.PostgreSQL)
    {
    }

    public PostgresTestContainerAdapter(string containerNamePrefix, int version)
        : base(containerNamePrefix,LatestForVersion(version), DatabaseProviders.PostgreSQL)
    {
    }

    public override string MigrationScriptsLocation => ".EF.Postgres.";

    public override IDbAdapter RespawnAdapter => DbAdapter.Postgres;

    protected override PostgreSqlBuilder CreateContainerBuilder()
    {
        return new PostgreSqlBuilder(Image);
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
