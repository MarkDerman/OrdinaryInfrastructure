using DotNet.Testcontainers.Containers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Respawn;
using System.Data.Common;
using Testcontainers.MsSql;
using Tests.Odin.DDD.Repositories.EF;

namespace Tests.Odin.DDD.Repositories.Database;

public class SqlServerDatabaseProviderAdapter : DatabaseProviderAdapterBase
{
    public SqlServerDatabaseProviderAdapter(int msSqlEditionYear = 2025)
        : base(LatestImage(msSqlEditionYear), DatabaseProviders.MicrosoftSQLServer)
    {
    }

    public SqlServerDatabaseProviderAdapter(string image)
        : base(image, DatabaseProviders.MicrosoftSQLServer)
    {
    }

    public override string MigrationScriptsLocation => ".EF.SqlServer.";

    public override IDbAdapter RespawnAdapter => DbAdapter.SqlServer;

    public override IDatabaseContainer BuildContainer()
    {
        return new MsSqlBuilder(Image).Build();
    }

    public override DbConnection CreateConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }

    public override void ConfigureDbContext(
        DbContextOptionsBuilder<TestDatabaseContext> options,
        string connectionString)
    {
        options.UseSqlServer(connectionString);
    }

    public static string LatestImage(int year)
    {
        return $"mcr.microsoft.com/mssql/server:{year}-latest";
    }
}
