using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using Microsoft.EntityFrameworkCore;
using Odin.System;
using Respawn;
using System.Data.Common;
using System.Reflection;
using Tests.Odin.DDD.Repositories.EF;

namespace Tests.Odin.DDD.Repositories.Database;

/// <summary>
/// Provides database-specific services needed by repository integration tests.
/// </summary>
public interface IDatabaseTestContainerAdapter
{
    /// <summary>
    /// PostgreSQL, SQLServer, SQLite, Oracle, etc.
    /// </summary>
    string DatabaseProvider { get; }

    /// <summary>
    /// Docker image url.
    /// </summary>
    DockerImage Image { get; }

    /// <summary>
    /// The Docker container name used.
    /// </summary>
    string ContainerName { get; }

    /// <summary>
    /// Location containing this provider's embedded database migration scripts.
    /// </summary>
    string MigrationScriptsLocation { get; }

    /// <summary>
    /// The Respawn adapter for this database provider.
    /// </summary>
    IDbAdapter RespawnAdapter { get; }

    /// <summary>
    /// Builds the Docker database container for this provider.
    /// </summary>
    /// <returns>The configured database container.</returns>
    IDatabaseContainer BuildContainer();

    /// <summary>
    /// Creates a database connection for this database provider.
    /// </summary>
    /// <param name="connectionString">The provider-specific connection string.</param>
    /// <returns>The database connection.</returns>
    DbConnection CreateConnection(string connectionString);

    /// <summary>
    /// Configures the EF Core provider for the test database context.
    /// </summary>
    /// <param name="options">The context options builder.</param>
    /// <param name="connectionString">The provider-specific connection string.</param>
    void ConfigureDbContext(DbContextOptionsBuilder<TestDatabaseContext> options, string connectionString);

    /// <summary>
    /// Runs this provider's embedded migration scripts.
    /// </summary>
    /// <param name="connectionString">The provider-specific connection string.</param>
    /// <param name="assemblyWithEmbeddedScripts">The assembly containing embedded SQL scripts.</param>
    /// <returns>The migration result.</returns>
    Task<Result> RunMigrationsAsync(string connectionString, Assembly assemblyWithEmbeddedScripts);
}
