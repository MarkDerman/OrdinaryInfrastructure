using Odin.DesignContracts;
using Odin.System;

namespace Odin.RemoteFiles;

/// <summary>
/// Standard factory for creating remote file sessions for fetching/listing/writing files from/in/to remote locations
/// Currently only supports SFTP sites
/// </summary>
public class RemoteFileSessionFactory : IRemoteFileSessionFactory
{
    private readonly Dictionary<string, Dictionary<string, string>> _fileSourceConnections;
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="remoteFilesOptions"></param>
    public RemoteFileSessionFactory(RemoteFilesOptions remoteFilesOptions)
    {
        Contract.Requires<ArgumentNullException>(remoteFilesOptions != null, "remoteFileConfiguration cannot be null");
        Contract.Requires<ArgumentNullException>(remoteFilesOptions.ConnectionStrings != null, "remoteFileConfiguration connection strings cannot null");
        
        _fileSourceConnections = remoteFilesOptions.ConnectionStrings.ToDictionary(
            kv => kv.Key, 
            kv => ConnectionSettingsHelper.ParseConnectionString(kv.Value, ';'));
    }
    
    /// <summary>
    /// Create a new remote file session based on the configuration for the connectionName passed.
    /// At the moment we are only supporting SFTP.
    /// HTTPS can be supported in future to fetch files from APIs
    /// FTP/S cannot be supported for our use case because the FTPS sites we want to consume seem to require SSL connection resumption, and this
    /// is not supported by FtpWebRequest in the dotnet runtime. Other option is to use the WINSCP library but this only supports windows.
    /// https://github.com/dotnet/runtime/issues/27916
    /// </summary>
    /// <param name="connectionName"></param>
    /// <returns></returns>
    public ResultValue<IRemoteFileSession> CreateRemoteFileSession(string connectionName)
    {
        Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(connectionName), "connectionName cannot be null");

        if (!_fileSourceConnections.ContainsKey(connectionName))
            return ResultValue<IRemoteFileSession>.Failure($"Connection name not supported or configured: {connectionName}");

        if (!_fileSourceConnections[connectionName].ContainsKey(ConnectionSettingsHelper.ProtocolKey) ||
            !Enum.TryParse(_fileSourceConnections[connectionName][ConnectionSettingsHelper.ProtocolKey], true, out ConnectionProtocol protocol))
            return ResultValue<IRemoteFileSession>.Failure(
                $"Unable to determine protocol from connection string. Connection: {connectionName}");

        return protocol switch
        {
            ConnectionProtocol.Sftp => ResultValue<IRemoteFileSession>.Success(
                new SftpRemoteFileSession(ConnectionSettingsHelper.ConstructSftpSettings(_fileSourceConnections[connectionName]))),
            
            ConnectionProtocol.Ftp => ResultValue<IRemoteFileSession>.Failure($"Protocol is not supported: {protocol}"),
            ConnectionProtocol.Https => ResultValue<IRemoteFileSession>.Failure($"Protocol is not supported: {protocol}"),
            _ => ResultValue<IRemoteFileSession>.Failure($"Protocol is not supported: {protocol}")
        };
    }
}