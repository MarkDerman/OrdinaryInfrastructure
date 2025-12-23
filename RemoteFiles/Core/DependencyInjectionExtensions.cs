using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Odin.DesignContracts;

namespace Odin.RemoteFiles;

/// <summary>
/// Dependency injection extensions
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adds RemoteFiles to dependency injection 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    public static IServiceCollection AddRemoteFiles(this IServiceCollection services, IConfiguration configuration,
        string sectionName = RemoteFilesOptions.DefaultConfigurationSectionName)
    {
        return AddRemoteFiles(services, configuration.GetSection(sectionName));
    }

    /// <summary>
    /// Adds RemoteFiles to dependency injection 
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configurationSection"></param>
    /// <returns></returns>
    /// <exception cref="ApplicationException"></exception>
    public static IServiceCollection AddRemoteFiles(this IServiceCollection services,
        IConfigurationSection configurationSection)
    {
        Contract.Requires(configurationSection!=null!, "Configuration Section for RemoteFiles cannot be null.");
        
        if (!configurationSection.Exists())
            throw new ApplicationException(
                $"Configuration section '{configurationSection.Key}' missing. Cannot configure remote files.");

        RemoteFilesOptions remoteFilesOptions = new RemoteFilesOptions();
        configurationSection.Bind(remoteFilesOptions);

        services.TryAddSingleton(remoteFilesOptions);
        services.TryAddSingleton<IRemoteFileSessionFactory, RemoteFileSessionFactory>();

        return services;
    }
}