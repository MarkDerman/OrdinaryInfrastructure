using Microsoft.Extensions.Configuration;

namespace Tests.Odin.Email;

public static class EmailTestConfiguration
{
    public static string GetTestEmailAddressFromConfig(IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return config["Email-TestToAddress"]!;
    }

    public static string GetTestFromNameFromConfig(IConfiguration config)
    {
        ArgumentNullException.ThrowIfNull(config);
        return config["Email-TestFromName"]!;
    }
}