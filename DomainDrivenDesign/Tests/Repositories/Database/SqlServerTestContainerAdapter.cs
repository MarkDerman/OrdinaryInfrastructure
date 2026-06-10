using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Respawn;
using System.Data.Common;
using Testcontainers.MsSql;
using Tests.Odin.DDD.Repositories.EF;

namespace Tests.Odin.DDD.Repositories.Database;

public class SqlServerTestContainerAdapter : DatabaseTestContainerAdapterBase
    <MsSqlBuilder, MsSqlContainer, MsSqlConfiguration>
{
    public SqlServerTestContainerAdapter(string containerNamePrefix, int msSqlEditionYear = 2025)
        : base(containerNamePrefix,LatestImage(msSqlEditionYear), DatabaseProviders.MSSQLServer)
    {
    }

    public SqlServerTestContainerAdapter(string containerNamePrefix, string image)
        : base(containerNamePrefix,image, DatabaseProviders.MSSQLServer)
    {
    }

    public override string MigrationScriptsLocation => ".Database.SqlServer.";

    public override IDbAdapter RespawnAdapter => DbAdapter.SqlServer;

    protected override MsSqlBuilder CreateContainerBuilder()
    {
        return new MsSqlBuilder(Image);
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

    protected override IEnumerable<string> SplitScript(string script)
    {
        StringReader reader = new StringReader(script);
        StringWriter batch = new StringWriter();

        while (reader.ReadLine() is { } line)
        {
            if (string.Equals(line.Trim(), "go", StringComparison.OrdinalIgnoreCase))
            {
                yield return batch.ToString();
                batch.GetStringBuilder().Clear();
                continue;
            }

            batch.WriteLine(line);
        }

        yield return batch.ToString();
    }
}
