using Odin.DesignContracts;

namespace Odin.RemoteFiles;


/// <summary>
/// Utility class for parsing connections strings, constructing connection settings classes, etc
/// </summary>
public static class ConnectionSettingsHelper
{
    /// <summary>
    /// key for the protocol specification within the connection string
    /// </summary>
    public const string ProtocolKey = "protocol";
    private const string HostKey = "host";
    private const string PortKey = "port";
    private const string UsernameKey = "username";
    private const string PasswordKey = "password";
    private const string PrivateKeyKey = "privatekey";
    private const string PrivateKeyPassphraseKey = "privatekeypassphrase";

    /// <summary>
    /// Parse a connection string in the format 'key=value;key=value...' to a dictionary
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="delimiter"></param>
    /// <returns></returns>
    public static Dictionary<string, string> ParseConnectionString(string connectionString, char delimiter)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        Dictionary<string, string> result = new Dictionary<string, string>();
        string[] keyValuePairs = connectionString.Trim(delimiter).Split(delimiter)
            .Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

        foreach (string keyValuePair in keyValuePairs)
        {
            string[] split = keyValuePair.Trim().Split('=');
            string key = split[0].Trim().ToLower();

            if (result.ContainsKey(key)) continue;

            string value = String.Join('=', split.Skip(1)).Trim();
            result.Add(key, value);
        }

        return result;
    }

    /// <summary>
    /// Initialise a SftpConnectionSettings class from a dictionary of key-value pairs
    /// </summary>
    /// <param name="connectionSettings"></param>
    /// <returns></returns>
    public static SftpConnectionSettings ConstructSftpSettings(Dictionary<string, string> connectionSettings)
    {
        // safeguard if called with keys containing upper case
        Dictionary<string, string> lowerCaseSettings =
            connectionSettings.ToDictionary(kv => kv.Key.ToLower(), kv => kv.Value);

        SftpConnectionSettings sftpSettings = new ();

        if (lowerCaseSettings.TryGetValue(HostKey, out string? setting))
            sftpSettings.Host = setting;

        if (lowerCaseSettings.TryGetValue(PortKey, out string? value) && int.TryParse(value, out int port))
            sftpSettings.Port = port;

        if (lowerCaseSettings.TryGetValue(UsernameKey, out string? caseSetting))
            sftpSettings.UserName = caseSetting;

        if (lowerCaseSettings.TryGetValue(PasswordKey, out string? lowerCaseSetting))
            sftpSettings.Password = lowerCaseSetting;

        if (lowerCaseSettings.TryGetValue(PrivateKeyKey, out string? pvtKey))
            sftpSettings.PrivateKey = pvtKey;

        if (lowerCaseSettings.TryGetValue(PrivateKeyPassphraseKey, out string? pvtPassphrase))
            sftpSettings.PrivateKeyPassphrase = pvtPassphrase;

        return sftpSettings;
    }
}