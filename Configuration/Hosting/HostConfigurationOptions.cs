using Microsoft.Extensions.Configuration;

namespace Odin.Configuration.Hosting;

/// <summary>
/// Controls how application configuration sources are added to a host.
/// </summary>
/// <remarks>
/// Defaults match Soulv host conventions: prefer <c>config/appSettings.json</c> when present,
/// fall back to <c>appSettings.json</c>, then add user secrets, environment variables, and Azure Key Vault.
/// </remarks>
public sealed class HostConfigurationOptions
{
    /// <summary>
    /// Gets or sets the JSON app settings file name to load.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>appSettings.json</c>. When <see cref="PreferConfigFolder"/> is enabled,
    /// this file is first looked up inside <see cref="ConfigFolderName"/>.
    /// </remarks>
    public string AppSettingsFileName { get; set; } = "appSettings.json";

    /// <summary>
    /// Gets or sets the folder name used for the preferred app settings location.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>config</c>, producing <c>config/appSettings.json</c>.
    /// </remarks>
    public string ConfigFolderName { get; set; } = "config";

    /// <summary>
    /// Gets or sets whether the loader should prefer the app settings file under <see cref="ConfigFolderName"/>.
    /// </summary>
    /// <remarks>
    /// When enabled, <c>config/appSettings.json</c> is loaded if it exists; otherwise
    /// the loader falls back to <c>appSettings.json</c> at the content root.
    /// </remarks>
    public bool PreferConfigFolder { get; set; } = true;

    /// <summary>
    /// Gets or sets the user secrets identifier to load.
    /// </summary>
    /// <remarks>
    /// When unset, the host application name passed to <c>AddHostConfiguration</c> is used.
    /// Set this when a host needs different local secrets while sharing the same app settings and Key Vault prefix.
    /// </remarks>
    public string? UserSecretsId { get; set; }

    /// <summary>
    /// Gets or sets whether environment variables are added after JSON files and user secrets.
    /// </summary>
    /// <remarks>
    /// Enabled by default so deployment environment values can override file and local secret values.
    /// </remarks>
    public bool AddEnvironmentVariables { get; set; } = true;

    /// <summary>
    /// Gets or sets an action that adds host-specific configuration sources.
    /// </summary>
    /// <remarks>
    /// The action runs after the main app settings file and before user secrets, environment variables, and Key Vault.
    /// It refines app defaults, not deployment secrets.
    /// Use this for extra files such as <c>serviceSettings.json</c>.
    /// </remarks>
    /// <example>
    /// e.g. a Windows service host can load shared app settings first, then layer service-specific defaults:
    /// <code>
    /// builder.Configuration.AddHostConfiguration("Flash.FinanceRecons", options =>
    /// {
    ///     options.AddExtraConfigurationSources = configuration =>
    ///         configuration.AddJsonFile("serviceSettings.json", optional: true, reloadOnChange: true);
    /// });
    /// </code>
    /// </example>
    public Action<ConfigurationManager>? AddExtraConfigurationSources { get; set; }

    /// <summary>
    /// Gets Azure Key Vault configuration options.
    /// </summary>
    /// <remarks>
    /// Key Vault is added last so vault secrets can override values from files, user secrets, and environment variables.
    /// </remarks>
    public KeyVaultConfigurationOptions KeyVault { get; } = new();
}
