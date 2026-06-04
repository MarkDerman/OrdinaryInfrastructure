using Odin.RemoteFiles;

namespace Tests.Odin.RemoteFiles;


public class ConnectionSettingsHelperTests
{
    [TestCase("")]
    [TestCase(null)]
    public void ParseConnectionString_throws_argument_null_exception_if_connection_string_is_malformed(
        string? connectionString)
    {
        Assert.Catch<Exception>(() => ConnectionSettingsHelper.ParseConnectionString(connectionString!, ';'));
    }
    [TestCase(";Host=ftp.co.za;UserName=bambi;Password=thisisthepassword;;;", ';', 3)]
    [TestCase(";Host=ftp.co.za; ;UserName=bambi;;Password=thisisthepassword;;;", ';', 3)]
    public void ParseConnectionString_ignores_multiple_consecutive_delimiters(string connectionString, char delimiter,
        int expectedCount)
    {
        Dictionary<string, string> parsedConnectionString =
            ConnectionSettingsHelper.ParseConnectionString(connectionString, delimiter);

        Assert.That(parsedConnectionString.Count, Is.EqualTo(expectedCount));
        Assert.That(parsedConnectionString.ContainsKey("host"), Is.True);
        Assert.That(parsedConnectionString.ContainsKey("username"), Is.True);
        Assert.That(parsedConnectionString.ContainsKey("password"), Is.True);
    }

    [Test]
    public void ParseConnectionString_supports_equals_symbol_in_value()
    {
        string connectionString = "Username=dale.warncke; password=thisis=a=password";
        Dictionary<string, string> parsedConnectionString =
            ConnectionSettingsHelper.ParseConnectionString(connectionString, ';');

        Assert.That(parsedConnectionString.Count, Is.EqualTo(2));
        Assert.That(parsedConnectionString.ContainsKey("username"), Is.True);
        Assert.That(parsedConnectionString.ContainsKey("password"), Is.True);
        Assert.That(parsedConnectionString["password"], Is.EqualTo("thisis=a=password"));
    }

    [Test]
    public void ParseConnectionString_leaves_casing_present_in_value()
    {
        string connectionString = "Username=dale.warncke; password=thisis=a=PASSword";
        Dictionary<string, string> parsedConnectionString =
            ConnectionSettingsHelper.ParseConnectionString(connectionString, ';');

        Assert.That(parsedConnectionString.Count, Is.EqualTo(2));
        Assert.That(parsedConnectionString.ContainsKey("username"), Is.True);
        Assert.That(parsedConnectionString.ContainsKey("password"), Is.True);
        Assert.That(parsedConnectionString["password"], Is.EqualTo("thisis=a=PASSword"));
    }
    [TestCase("Host", "test.flash.co.za")]
    [TestCase("host", "test.flash.co.za")]
    [TestCase("Port", "30")]
    [TestCase("port", "30")]
    [TestCase("UserName", "mark.derman")]
    [TestCase("userName", "mark.derman")]
    [TestCase("Password", "He_likes-to(kitesurf)")]
    [TestCase("password", "He_likes-to(kitesurf)")]
    [TestCase("PrivateKey", "the/super/private.key")]
    [TestCase("privatekey", "the/super/private.key")]
    [TestCase("PrivateKeyPassphrase", "This_is_the_fancy_passphrase")]
    [TestCase("privatekeypassphrase", "This_is_the_fancy_passphrase")]
    public void ConstructSftpSettings_sets_properties_correctly(string propertyName, string value)
    {
        SftpConnectionSettings result =
            ConnectionSettingsHelper.ConstructSftpSettings(new Dictionary<string, string>() { { propertyName, value } });

        switch (propertyName.ToLower())
        {
            case "host":
                {
                    Assert.That(result.Host, Is.EqualTo("test.flash.co.za"));
                    Assert.That(result.Port, Is.EqualTo(22));
                    Assert.That(result.UserName, Is.Null);
                    Assert.That(result.Password, Is.Null);
                    Assert.That(result.PrivateKey, Is.Null);
                    Assert.That(result.PrivateKeyPassphrase, Is.Null);
                    break;
                }
            case "port":
                {
                    Assert.That(result.Port, Is.EqualTo(30));
                    Assert.That(result.Host, Is.Null);
                    Assert.That(result.UserName, Is.Null);
                    Assert.That(result.Password, Is.Null);
                    Assert.That(result.PrivateKey, Is.Null);
                    Assert.That(result.PrivateKeyPassphrase, Is.Null);
                    break;
                }
            case "username":
                {
                    Assert.That(result.UserName, Is.EqualTo("mark.derman"));
                    Assert.That(result.Port, Is.EqualTo(22));
                    Assert.That(result.Host, Is.Null);
                    Assert.That(result.Password, Is.Null);
                    Assert.That(result.PrivateKey, Is.Null);
                    Assert.That(result.PrivateKeyPassphrase, Is.Null);
                    break;
                }
            case "password":
                {
                    Assert.That(result.Password, Is.EqualTo("He_likes-to(kitesurf)"));
                    Assert.That(result.Port, Is.EqualTo(22));
                    Assert.That(result.Host, Is.Null);
                    Assert.That(result.UserName, Is.Null);
                    Assert.That(result.PrivateKey, Is.Null);
                    Assert.That(result.PrivateKeyPassphrase, Is.Null);
                    break;
                }
            case "privatekey":
                {
                    Assert.That(result.PrivateKey, Is.EqualTo("the/super/private.key"));
                    Assert.That(result.Port, Is.EqualTo(22));
                    Assert.That(result.Host, Is.Null);
                    Assert.That(result.UserName, Is.Null);
                    Assert.That(result.Password, Is.Null);
                    Assert.That(result.PrivateKeyPassphrase, Is.Null);
                    break;
                }
            case "privatekeypassphrase":
                {
                    Assert.That(result.PrivateKeyPassphrase, Is.EqualTo("This_is_the_fancy_passphrase"));
                    Assert.That(result.Port, Is.EqualTo(22));
                    Assert.That(result.Host, Is.Null);
                    Assert.That(result.UserName, Is.Null);
                    Assert.That(result.Password, Is.Null);
                    Assert.That(result.PrivateKey, Is.Null);
                    break;
                }
        }
    }
}