using DotNet.Testcontainers.Containers;
using Testcontainers.MsSql;

namespace Tests.Odin.DDD.Repositories.Database;

public class SqlServerContainerBuilder : DatabaseContainerBuilderBase
{
    public SqlServerContainerBuilder(int msSqlEditionYear = 2025)
        : base(LatestImage(msSqlEditionYear),DatabaseProviders.MicrosoftSQLServer)
    {
    }
    public SqlServerContainerBuilder(string image) : 
        base(image,DatabaseProviders.MicrosoftSQLServer)
    {
    }

    public override IDatabaseContainer Build()
    {
        return new MsSqlBuilder(Image).Build();
    }

    public static string LatestImage(int year) => $"mcr.microsoft.com/mssql/server:{year}-latest";
}