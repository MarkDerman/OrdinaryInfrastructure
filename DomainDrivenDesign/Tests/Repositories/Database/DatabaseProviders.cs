using Ardalis.SmartEnum;

namespace Tests.Odin.DDD.Repositories.Database;

public class DatabaseProvider : SmartEnum<DatabaseProvider, int>
{
    public string Image { get; private set; }
    public static readonly DatabaseProvider SqlServer2022 = 
        new DatabaseProvider("SqlServer2022", 1, "mcr.microsoft.com/mssql/server:2022-latest");
    public static readonly DatabaseProvider SqlServer2019 = 
        new DatabaseProvider("SqlServer2019", 2, "mcr.microsoft.com/mssql/server:2019-latest");

    private DatabaseProvider(string name, int index, string image) : base(name, index)
    {
        Image = image;
    }

    public static IEnumerable<DatabaseProvider> AllProviders()
    {
        for (int i = 1; i <= 2; i++)
        {
            yield return FromValue(i);
        }
    }
    
}