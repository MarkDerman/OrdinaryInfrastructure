using DotNet.Testcontainers.Containers;

namespace Tests.Odin.DDD.Repositories.Database;

/// <inheritdoc />
public abstract class DatabaseContainerBuilderBase : IDatabaseContainerBuilder
{
    protected DatabaseContainerBuilderBase(string image, string databaseProvider)
    {
        Image = image;
        DatabaseProvider = databaseProvider;
    }

    /// <inheritdoc />
    public string Image { get; }

    /// <inheritdoc />
    public string DatabaseProvider { get; }

    /// <inheritdoc />
    public abstract IDatabaseContainer Build();
}