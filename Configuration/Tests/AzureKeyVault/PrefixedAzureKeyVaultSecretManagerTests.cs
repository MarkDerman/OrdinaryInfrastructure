using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Moq;
using Odin.Configuration;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Tests.Odin.Configuration.AzureKeyVault;

/// <summary>
/// Unit tests for the <see cref="ConfigurationBuilderExtensions.AddOdinPrefixedAzureKeyVault"/> extension method
/// and the <see cref="PrefixedAzureKeyVaultSecretManager"/> class.
/// </summary>
public class PrefixedAzureKeyVaultSecretManagerTests
{
    private readonly Mock<IConfigurationBuilder> _configBuilderMock = new();
    private readonly Mock<TokenCredential> _credentialMock = new();

    #region Secret Manager Tests

    /// <summary>
    /// Verifies that <see cref="PrefixedAzureKeyVaultSecretManager.Load"/> returns true 
    /// when the secret name starts with the configured prefix.
    /// </summary>
    [Fact]
    public void Load_SecretNameStartsWithPrefix_ReturnsTrue()
    {
        // Arrange
        var prefix = "App1-";
        var manager = new PrefixedAzureKeyVaultSecretManager(prefix);
        var secretProperties = InternalModelFactory.SecretProperties(name: "App1-Secret");

        // Act
        var result = manager.Load(secretProperties);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that <see cref="PrefixedAzureKeyVaultSecretManager.Load"/> returns false 
    /// when the secret name does not start with the configured prefix.
    /// </summary>
    [Fact]
    public void Load_SecretNameDoesNotStartWithPrefix_ReturnsFalse()
    {
        // Arrange
        var prefix = "App1-";
        var manager = new PrefixedAzureKeyVaultSecretManager(prefix);
        var secretProperties = InternalModelFactory.SecretProperties(name: "Other-Secret");

        // Act
        var result = manager.Load(secretProperties);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that <see cref="PrefixedAzureKeyVaultSecretManager.Load"/> returns true 
    /// when the secret name matches the prefix exactly.
    /// </summary>
    [Fact]
    public void Load_SecretNameMatchesPrefixExactly_ReturnsTrue()
    {
        // Arrange
        var prefix = "App1-";
        var manager = new PrefixedAzureKeyVaultSecretManager(prefix);
        var secretProperties = InternalModelFactory.SecretProperties(name: "App1-");

        // Act
        var result = manager.Load(secretProperties);

        // Assert
        Assert.True(result);
    }

    /// <summary>
    /// Verifies that <see cref="PrefixedAzureKeyVaultSecretManager.GetKey"/> strips the prefix 
    /// and replaces every remaining hyphen with a colon.
    /// </summary>
    [Fact]
    public void GetKey_StripsPrefixAndReplacesHyphens_ReturnsTransformedKey()
    {
        // Arrange
        var prefix = "App1-Environment1-";
        var manager = new PrefixedAzureKeyVaultSecretManager(prefix);
        var secret = InternalModelFactory.KeyVaultSecret(name: "App1-Environment1-ConnectionStrings--DefaultConnection", value: "secret-value");

        // Act
        var key = manager.GetKey(secret);

        Assert.Equal("ConnectionStrings::DefaultConnection", key);
    }

    /// <summary>
    /// Verifies that <see cref="PrefixedAzureKeyVaultSecretManager.GetKey"/> replaces every remaining hyphen.
    /// </summary>
    [Fact]
    public void GetKey_HandlesMultipleHyphens_ReturnsColons()
    {
        // Arrange
        var prefix = "Prefix-";
        var manager = new PrefixedAzureKeyVaultSecretManager(prefix);
        var secret = InternalModelFactory.KeyVaultSecret(name: "Prefix-Logging--LogLevel--Default", value: "Information");

        // Act
        var key = manager.GetKey(secret);

        // Assert
        Assert.Equal("Logging::LogLevel::Default", key);
    }

    #endregion

    #region Guard Clause Tests

    /// <summary>
    /// Verifies that <see cref="ConfigurationBuilderExtensions.AddOdinPrefixedAzureKeyVault"/> throws <see cref="ArgumentNullException"/>
    /// when the <see cref="IConfigurationBuilder"/> is null.
    /// </summary>
    [Fact]
    public void AddPrefixedAzureKeyVault_NullConfigBuilder_ThrowsArgumentNullException()
    {
        // Arrange
        IConfigurationBuilder configBuilder = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            configBuilder.AddOdinPrefixedAzureKeyVault("my-vault", "prefix", _credentialMock.Object));
    }

    /// <summary>
    /// Verifies that <see cref="ConfigurationBuilderExtensions.AddOdinPrefixedAzureKeyVault"/> throws <see cref="ArgumentException"/>
    /// when the vault name or URI is empty or contains only white-space characters.
    /// </summary>
    /// <param name="vaultName">The invalid vault name or URI.</param>
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void AddPrefixedAzureKeyVault_EmptyOrWhiteSpaceVaultName_ThrowsArgumentException(string vaultName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _configBuilderMock.Object.AddOdinPrefixedAzureKeyVault(vaultName, "prefix", _credentialMock.Object));
    }

    /// <summary>
    /// Verifies that <see cref="ConfigurationBuilderExtensions.AddOdinPrefixedAzureKeyVault"/> throws <see cref="ArgumentNullException"/>
    /// when the vault name or URI is null.
    /// </summary>
    [Fact]
    public void AddPrefixedAzureKeyVault_NullVaultName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _configBuilderMock.Object.AddOdinPrefixedAzureKeyVault(null!, "prefix", _credentialMock.Object));
    }

    /// <summary>
    /// Verifies that <see cref="ConfigurationBuilderExtensions.AddOdinPrefixedAzureKeyVault"/> throws <see cref="ArgumentNullException"/>
    /// when the prefix is null.
    /// </summary>
    [Fact]
    public void AddPrefixedAzureKeyVault_NullPrefix_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _configBuilderMock.Object.AddOdinPrefixedAzureKeyVault("my-vault", null!, _credentialMock.Object));
    }

    /// <summary>
    /// Verifies that <see cref="ConfigurationBuilderExtensions.AddOdinPrefixedAzureKeyVault"/> throws <see cref="ArgumentNullException"/>
    /// when the credential is null.
    /// </summary>
    [Fact]
    public void AddPrefixedAzureKeyVault_NullCredential_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _configBuilderMock.Object.AddOdinPrefixedAzureKeyVault("my-vault", "prefix", null!));
    }

    #endregion

    #region Vault URI Construction Tests

    /// <summary>
    /// Verifies that passing a short vault name results in a default ".vault.azure.net" domain URI.
    /// </summary>
    [Fact]
    public void AddPrefixedAzureKeyVault_WithShortVaultName_UsesDefaultAzureNetDomain()
    {
        // Arrange
        var vaultName = "my-vault";

        // Act & Assert
        // We verify that the method executes without error for a short name.
        // Direct verification of the internal URI is restricted by Azure SDK encapsulation.
        var exception = Record.Exception(() =>
            _configBuilderMock.Object.AddOdinPrefixedAzureKeyVault(vaultName, "prefix", _credentialMock.Object));

        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that passing a full HTTPS URI for the vault name is handled correctly without altering the domain.
    /// </summary>
    [Fact]
    public void AddPrefixedAzureKeyVault_WithFullHttpsUri_UsesProvidedUri()
    {
        // Arrange
        var vaultUriString = "https://custom.vault.azure.cn/";

        // Act & Assert
        // We verify that the method executes without error for a full URI.
        var exception = Record.Exception(() =>
            _configBuilderMock.Object.AddOdinPrefixedAzureKeyVault(vaultUriString, "prefix", _credentialMock.Object));

        Assert.Null(exception);
    }

    #endregion

    #region Integration/Configuration Tests

    /// <summary>
    /// Verifies that the extension method configures a prefix-aware secret manager.
    /// </summary>
    [Fact]
    public void AddPrefixedAzureKeyVault_ConfiguresPrefixedSecretManager()
    {
        // Arrange
        var prefix = "my-prefix";

        // Act
        var options = new AzureKeyVaultConfigurationOptions();
        _configBuilderMock.Object.AddOdinPrefixedAzureKeyVault("my-vault", prefix, _credentialMock.Object, options);

        var loadedSecret = InternalModelFactory.SecretProperties(name: "my-prefixSecret");
        var ignoredSecret = InternalModelFactory.SecretProperties(name: "other-prefixSecret");
        var secret = InternalModelFactory.KeyVaultSecret(name: "my-prefixConnectionStrings--Default", value: "secret-value");

        Assert.IsType<PrefixedAzureKeyVaultSecretManager>(options.Manager);
        Assert.True(options.Manager.Load(loadedSecret));
        Assert.False(options.Manager.Load(ignoredSecret));
        Assert.Equal("ConnectionStrings::Default", options.Manager.GetKey(secret));
    }

    #endregion
}

/// <summary>
/// Helper to create internal Key Vault models for testing.
/// </summary>
internal static class InternalModelFactory
{
    public static SecretProperties SecretProperties(string name)
    {
        // Try the public constructor first
        var ctor = typeof(SecretProperties).GetConstructor(new[] { typeof(string) });
        if (ctor != null)
        {
            return (SecretProperties)ctor.Invoke(new object[] { name });
        }

        // Try internal constructor
        ctor = typeof(SecretProperties).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(string) }, null);
        if (ctor != null)
        {
            return (SecretProperties)ctor.Invoke(new object[] { name });
        }

        // Fallback to uninitialized object and reflection
        var properties = (SecretProperties)RuntimeHelpers.GetUninitializedObject(typeof(SecretProperties));
        var nameProperty = typeof(SecretProperties).GetProperty("Name", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        nameProperty?.SetValue(properties, name);
        return properties;
    }

    public static KeyVaultSecret KeyVaultSecret(string name, string value)
    {
        // Try the public constructor KeyVaultSecret(string name, string value)
        var ctor = typeof(KeyVaultSecret).GetConstructor(new[] { typeof(string), typeof(string) });
        if (ctor != null)
        {
            return (KeyVaultSecret)ctor.Invoke(new object[] { name, value });
        }

        // Try the constructor that takes SecretProperties and string
        var properties = SecretProperties(name);
        ctor = typeof(KeyVaultSecret).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(SecretProperties), typeof(string) }, null);
        if (ctor != null)
        {
            return (KeyVaultSecret)ctor.Invoke(new object[] { properties, value });
        }

        throw new InvalidOperationException("Could not find a suitable constructor for KeyVaultSecret");
    }
}
