namespace Odin.RemoteFiles;

/// <summary>
/// Configuration class for binding application settings from configuration.
/// Configuration is expected to be passed to extension method for registering remote files support
/// and must contain a section "RemoteFiles" with a list of named connection strings
/// with supported protocols as per the IRemoteFileSessionFactory
/// </summary>
public sealed record RemoteFilesOptions
{
    /// <summary>
    /// The default configuration section name for RemoteFiles options.
    /// </summary>
    public const string DefaultConfigurationSectionName = "RemoteFiles";

    /// <summary>
    /// transfer.flash.co.za and any other FTP\SFTP file sources
    /// connection strings must be in the form of key-value pairs
    /// "transfer.flash.co.za": "Protocol=ftp;Host=transfer.flash.co.za;Port=21;UserName=username;Password=some_password"
    /// </summary>
    public Dictionary<string, string> ConnectionStrings { get; set; }
}