using DotNet.Testcontainers.Containers;
using Testcontainers.PostgreSql;

namespace Tests.Odin.DDD.Repositories.Database;

public class PostgresContainerBuilder : DatabaseContainerBuilderBase
{
    public PostgresContainerBuilder(string image) : 
        base(image,DatabaseProviders.PostgreSQL)
    {
    }
    public PostgresContainerBuilder(int version) : 
        base(LatestForVersion(version),DatabaseProviders.PostgreSQL)
    {
    }
    public override IDatabaseContainer Build()
    {
        return new PostgreSqlBuilder(Image).Build();
    }

    public static string LatestForVersion(int version) => $"postgres:{version}";
}