using Odin.Strings;

namespace Tests.Odin.DDD.Repositories.Database;

public class DatabaseProviders : StringEnum<DatabaseProviders>
{
    public const string PostgreSQL = "PostgreSQL";
    public const string MSSQLServer = "MSSQLServer";
    public const string SQLite = "SQLite";
    public const string Oracle = "Oracle";
}