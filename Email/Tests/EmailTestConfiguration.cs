using Microsoft.Extensions.Configuration;
using Odin.DesignContracts;

namespace Tests.Odin.Email;

public static class EmailTestConfiguration
{
    public static string GetTestEmailAddressFromConfig(IConfiguration config)
    {
        Precondition.RequiresNotNull(config);
        return config["Email-TestToAddress"]!;
    }

    public static string GetTestFromNameFromConfig(IConfiguration config)
    {
        Precondition.RequiresNotNull(config);
        return config["Email-TestFromName"]!;
    }
}