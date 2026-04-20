using Odin.RemoteFiles;

namespace Tests.Odin.RemoteFiles;


public class ConnectionSettingsHelperTests
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void ParseConnectionString_throws_argument_null_exception_if_connection_string_is_malformed(
        string? connectionString)
    {
        Assert.Throws<ArgumentNullException>(() => ConnectionSettingsHelper.ParseConnectionString(connectionString!, ';'));
    }

    [Theory]
    [InlineData(";Host=ftp.co.za;UserName=bambi;Password=thisisthepassword;;;", ';', 3)]
    [InlineData(";Host=ftp.co.za; ;UserName=bambi;;Password=thisisthepassword;;;", ';', 3)]
    public void ParseConnectionString_ignores_multiple_consecutive_delimiters(string connectionString, char delimiter,
        int expectedCount)
    {
        Dictionary<string, string> parsedConnectionString =
            ConnectionSettingsHelper.ParseConnectionString(connectionString, delimiter);
        
        Assert.Equal(expectedCount, parsedConnectionString.Count);
        Assert.True(parsedConnectionString.ContainsKey("host"));
        Assert.True(parsedConnectionString.ContainsKey("username"));
        Assert.True(parsedConnectionString.ContainsKey("password"));
    }
    
    [Fact]
    public void ParseConnectionString_supports_equals_symbol_in_value()
    {
        string connectionString = "Username=dale.warncke; password=thisis=a=password";
        Dictionary<string, string> parsedConnectionString =
            ConnectionSettingsHelper.ParseConnectionString(connectionString, ';');
        
        Assert.Equal(2, parsedConnectionString.Count);
        Assert.True(parsedConnectionString.ContainsKey("username"));
        Assert.True(parsedConnectionString.ContainsKey("password"));
        Assert.Equal("thisis=a=password", parsedConnectionString["password"]);
    }
    
    [Fact]
    public void ParseConnectionString_leaves_casing_present_in_value()
    {
        string connectionString = "Username=dale.warncke; password=thisis=a=PASSword";
        Dictionary<string, string> parsedConnectionString =
            ConnectionSettingsHelper.ParseConnectionString(connectionString, ';');
        
        Assert.Equal(2, parsedConnectionString.Count);
        Assert.True(parsedConnectionString.ContainsKey("username"));
        Assert.True(parsedConnectionString.ContainsKey("password"));
        Assert.Equal("thisis=a=PASSword", parsedConnectionString["password"]);
    }

    [Theory]
    [InlineData("Host", "test.flash.co.za")]
    [InlineData("host", "test.flash.co.za")]
    [InlineData("Port", "30")]
    [InlineData("port", "30")]
    [InlineData("UserName", "mark.derman")]
    [InlineData("userName", "mark.derman")]
    [InlineData("Password", "He_likes-to(kitesurf)")]
    [InlineData("password", "He_likes-to(kitesurf)")]
    [InlineData("PrivateKey", "the/super/private.key")]
    [InlineData("privatekey", "the/super/private.key")]
    [InlineData("PrivateKeyPassphrase", "This_is_the_fancy_passphrase")]
    [InlineData("privatekeypassphrase", "This_is_the_fancy_passphrase")]
    public void ConstructSftpSettings_sets_properties_correctly(string propertyName, string value)
    {
        SftpConnectionSettings result =
            ConnectionSettingsHelper.ConstructSftpSettings(new Dictionary<string, string>() { { propertyName, value }});

        switch (propertyName.ToLower())
        {
            case "host":
            {
                Assert.Equal("test.flash.co.za", result.Host);
                Assert.Equal(22, result.Port);
                Assert.Null(result.UserName);
                Assert.Null(result.Password);
                Assert.Null(result.PrivateKey);
                Assert.Null(result.PrivateKeyPassphrase);
                break;
            }
            case "port":
            {
                Assert.Equal(30, result.Port);
                Assert.Null(result.Host);
                Assert.Null(result.UserName);
                Assert.Null(result.Password);
                Assert.Null(result.PrivateKey);
                Assert.Null(result.PrivateKeyPassphrase);
                break;
            }
            case "username":
            {
                Assert.Equal("mark.derman", result.UserName);
                Assert.Equal(22, result.Port);
                Assert.Null(result.Host);
                Assert.Null(result.Password);
                Assert.Null(result.PrivateKey);
                Assert.Null(result.PrivateKeyPassphrase);
                break;
            }
            case "password":
            {
                Assert.Equal("He_likes-to(kitesurf)", result.Password);
                Assert.Equal(22, result.Port);
                Assert.Null(result.Host);
                Assert.Null(result.UserName);
                Assert.Null(result.PrivateKey);
                Assert.Null(result.PrivateKeyPassphrase);
                break;
            }
            case "privatekey":
            {
                Assert.Equal("the/super/private.key", result.PrivateKey);
                Assert.Equal(22, result.Port);
                Assert.Null(result.Host);
                Assert.Null(result.UserName);
                Assert.Null(result.Password);
                Assert.Null(result.PrivateKeyPassphrase);
                break;
            }
            case "privatekeypassphrase":
            {
                Assert.Equal("This_is_the_fancy_passphrase", result.PrivateKeyPassphrase);
                Assert.Equal(22, result.Port);
                Assert.Null(result.Host);
                Assert.Null(result.UserName);
                Assert.Null(result.Password);
                Assert.Null(result.PrivateKey);
                break;
            }
        }
    }
}
