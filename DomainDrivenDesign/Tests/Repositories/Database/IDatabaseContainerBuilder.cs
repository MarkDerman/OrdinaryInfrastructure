using DotNet.Testcontainers.Containers;

namespace Tests.Odin.DDD.Repositories.Database;

/// <summary>
/// Abstracts creating an IDatabaseContainer for Postgres, SqlServer, etc.
/// </summary>
public interface IDatabaseContainerBuilder
{
    /// <summary>
    /// PostgreSQL, SQLServer, SQLite, Oracle, etc.
    /// </summary>
    string DatabaseProvider { get; }
    
    /// <summary>
    /// Docker image url
    /// </summary>
    string Image { get; }
    
    /// <summary>
    /// Builds the Docker container, but does not start it.
    /// </summary>
    /// <returns></returns>
    IDatabaseContainer Build();
}