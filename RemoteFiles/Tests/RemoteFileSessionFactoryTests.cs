using NUnit.Framework;
using Odin.RemoteFiles;
using Odin.System;


namespace Tests.Odin.RemoteFiles;


[TestFixture]
public class RemoteFileSessionFactoryTests
{
    [Test]
    [TestCase("")]
    [TestCase(null)]
    public void CreateRemoteFileSession_throws_exception_if_connection_name_is_malformed(string connectionName)
    {
        RemoteFilesOptions remoteFileConfig = new RemoteFilesOptions
        {
            ConnectionStrings = new Dictionary<string, string>()
        };
        RemoteFileSessionFactory sut = new RemoteFileSessionFactory(remoteFileConfig);
        Assert.Throws<ArgumentNullException>(() => sut.CreateRemoteFileSession(connectionName));
    }
    
    [Test]
    public void CreateRemoteFileSession_fails_gracefully_if_connection_name_is_not_configured()
    {
        RemoteFilesOptions remoteFileConfig = new RemoteFilesOptions
        {
            ConnectionStrings = new Dictionary<string, string>()
        };
        RemoteFileSessionFactory sut = new RemoteFileSessionFactory(remoteFileConfig);
        
        ResultValue<IRemoteFileSession> result = sut.CreateRemoteFileSession("test.connection.co.za");
        
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.MessagesToString(), Contains.Substring("Connection name not supported or configured: test.connection.co.za"));
    }
    
    [Test]
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
        
        Assert.That(result.IsSuccess, Is.False);    
        Assert.That(result.MessagesToString(), Is.EqualTo("Unable to determine protocol from connection string. Connection: test.connection.co.za"));
    }
    
    [Test]
    [TestCase("TCP")]
    [TestCase("AMQP")]
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
        
        Assert.That(result.IsSuccess, Is.False);   
        Assert.That(result.MessagesToString(), Is.EqualTo("Unable to determine protocol from connection string. Connection: test.connection.co.za"));
    }
    
    [Test]
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
        
        Assert.That( result.IsSuccess, Is.False);          
        Assert.That(result.MessagesToString(), Is.EqualTo($"Protocol is not supported: {ConnectionProtocol.Https}"));
    }
    
    [Test]
    [TestCase(ConnectionProtocol.Sftp, typeof(SftpRemoteFileSession))]
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
        
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.GetType(), Is.EqualTo(resultType));
    }
}