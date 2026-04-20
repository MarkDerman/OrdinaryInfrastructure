using Odin.RemoteFiles;
using Odin.System;


namespace Tests.Odin.RemoteFiles;


public class RemoteFileSessionFactoryTests
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void CreateRemoteFileSession_throws_exception_if_connection_name_is_malformed(string? connectionName)
    {
        RemoteFilesOptions remoteFileConfig = new RemoteFilesOptions
        {
            ConnectionStrings = new Dictionary<string, string>()
        };
        RemoteFileSessionFactory sut = new RemoteFileSessionFactory(remoteFileConfig);
        Assert.Throws<ArgumentNullException>(() => sut.CreateRemoteFileSession(connectionName!));
    }
    
    [Fact]
    public void CreateRemoteFileSession_fails_gracefully_if_connection_name_is_not_configured()
    {
        RemoteFilesOptions remoteFileConfig = new RemoteFilesOptions
        {
            ConnectionStrings = new Dictionary<string, string>()
        };
        RemoteFileSessionFactory sut = new RemoteFileSessionFactory(remoteFileConfig);
        
        ResultValue<IRemoteFileSession> result = sut.CreateRemoteFileSession("test.connection.co.za");
        
        Assert.False(result.IsSuccess);
        Assert.Contains("Connection name not supported or configured: test.connection.co.za", result.MessagesToString());
    }
    
    [Fact]
    public void CreateRemoteFileSession_fails_gracefully_if_connection_setting_is_missing_protocol()
    {
        RemoteFilesOptions remoteFileConfig = new RemoteFilesOptions
        {
            ConnectionStrings = new Dictionary<string, string>
            {
                { "test.connection.co.za", "Host=test.connection.co.za;UserName=dale.warncke@flash.co.za"}
            }
        };
        RemoteFileSessionFactory sut = new RemoteFileSessionFactory(remoteFileConfig);
        
        ResultValue<IRemoteFileSession> result = sut.CreateRemoteFileSession("test.connection.co.za");
        
        Assert.False(result.IsSuccess);
        Assert.Equal("Unable to determine protocol from connection string. Connection: test.connection.co.za", result.MessagesToString());
    }
    
    [Theory]
    [InlineData("TCP")]
    [InlineData("AMQP")]
    public void CreateRemoteFileSession_fails_gracefully_if_protocol_cannot_be_parsed_to_enum(string protocol)
    {
        RemoteFilesOptions remoteFileConfig = new RemoteFilesOptions
        {
            ConnectionStrings = new Dictionary<string, string>
            {
                { "test.connection.co.za", $"Protocol={protocol};Host=test.connection.co.za;UserName=dale.warncke@flash.co.za"}
            }
        };
        RemoteFileSessionFactory sut = new RemoteFileSessionFactory(remoteFileConfig);
        
        ResultValue<IRemoteFileSession> result = sut.CreateRemoteFileSession("test.connection.co.za");
        
        Assert.False(result.IsSuccess);
        Assert.Equal("Unable to determine protocol from connection string. Connection: test.connection.co.za", result.MessagesToString());
    }
    
    [Fact]
    public void CreateRemoteFileSession_fails_gracefully_if_protocol_is_not_supported()
    {
        RemoteFilesOptions remoteFileConfig = new RemoteFilesOptions
        {
            ConnectionStrings = new Dictionary<string, string>
            {
                { "test.connection.co.za", $"Protocol={ConnectionProtocol.Https};Host=test.connection.co.za;UserName=dale.warncke@flash.co.za"}
            }
        };
        RemoteFileSessionFactory sut = new RemoteFileSessionFactory(remoteFileConfig);
        
        ResultValue<IRemoteFileSession> result = sut.CreateRemoteFileSession("test.connection.co.za");
        
        Assert.False(result.IsSuccess);
        Assert.Equal($"Protocol is not supported: {ConnectionProtocol.Https}", result.MessagesToString());
    }
    
    [Theory]
    [InlineData(ConnectionProtocol.Sftp, typeof(SftpRemoteFileSession))]
    public void CreateRemoteFileSession_successfully_creates_file_providers(ConnectionProtocol protocol, Type resultType)
    {
        RemoteFilesOptions remoteFileConfig = new RemoteFilesOptions
        {
            ConnectionStrings = new Dictionary<string, string>
            {
                { "test.connection.co.za", $"Protocol={protocol};Host=test.connection.co.za;UserName=dale.warncke@flash.co.za"}
            }
        };
        RemoteFileSessionFactory sut = new RemoteFileSessionFactory(remoteFileConfig);
        
        ResultValue<IRemoteFileSession> result = sut.CreateRemoteFileSession("test.connection.co.za");
        
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.IsType(resultType, result.Value);
    }
}
