using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Odin.BackgroundProcessing;
using Odin.DesignContracts;
using Odin.System;
using Odin.Utility;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Dependency injection methods to support Background Processing services setup by adding an IBackgroundProcessor
    /// from configuration
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Adds BackgroundProcessing services
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <param name="configurationSection"></param>
        /// <param name="sqlServerConnectionString"></param>
        public static void AddOdinBackgroundProcessing(this IServiceCollection serviceCollection, 
            IConfiguration configuration, IConfigurationSection configurationSection, string? sqlServerConnectionString = null)
        {
            AddOdinBackgroundProcessing(serviceCollection, configuration, configurationSection, sqlServerConnectionString is null ? null : _ => sqlServerConnectionString);
        }
        
        /// <summary>
        /// B - Adds BackgroundProcessing services (such as Hangfire's server) according to the provided ConfigurationSection
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <param name="sectionName">Cryptography by default</param>
        /// <param name="sqlServerConnectionString"></param>
        public static void AddOdinBackgroundProcessing(this IServiceCollection serviceCollection,
            IConfiguration configuration, string sectionName = Constants.ModuleNoun, string? sqlServerConnectionString = null)
        {
            IConfigurationSection section = configuration.GetSection(sectionName);
            if (section.Value == null && !section.GetChildren().Any())
            {
                throw new ApplicationException(
                    $"{nameof(AddOdinBackgroundProcessing)}: Section {sectionName} missing in configuration.");
            }
        
            serviceCollection.AddOdinBackgroundProcessing(configuration, section, sqlServerConnectionString);
        }

        /// <summary>
        /// A - Adds BackgroundProcessing services
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <param name="sectionName"></param>
        /// <param name="sqlServerConnectionStringFactory"></param>
        /// <exception cref="ApplicationException"></exception>
        public static void AddOdinBackgroundProcessing(this IServiceCollection serviceCollection,
            IConfiguration configuration, string sectionName, Func<IServiceProvider, string>? sqlServerConnectionStringFactory = null)
        {
            IConfigurationSection section = configuration.GetSection(sectionName);
            if (section.Value == null && !section.GetChildren().Any())
            {
                throw new ApplicationException(
                    $"{nameof(AddOdinBackgroundProcessing)}: Section {sectionName} missing in configuration.");
            }

            serviceCollection.AddOdinBackgroundProcessing(configuration, section, sqlServerConnectionStringFactory);
        }

        /// <summary>
        /// Adds BackgroundProcessing services (such as Hangfire's server) according to the provided ConfigurationSection 
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <param name="configurationSection"></param>
        /// <param name="sqlServerConnectionStringFactory"></param>
        /// <returns></returns>
        public static void AddOdinBackgroundProcessing(
            this IServiceCollection serviceCollection, IConfiguration configuration,
            IConfigurationSection configurationSection, Func<IServiceProvider, string>? sqlServerConnectionStringFactory = null)
        {
            Contract.RequiresNotNull(configurationSection);

            BackgroundProcessingOptions options = new BackgroundProcessingOptions();
            configurationSection.Bind(options);

            Result configResult = options.Validate();
            if (!configResult.IsSuccess)
            {
                throw new ApplicationException(
                    $"{nameof(AddOdinBackgroundProcessing)}: Invalid BackgroundProcessing configuration. Errors are: {configResult.MessagesToString()}");
            }

            serviceCollection.TryAddSingleton(options);


            if (options.Provider == BackgroundProcessingProviders.Null)
            {
                serviceCollection.AddTransient<IBackgroundProcessor, NullBackgroundProcessor>();
                return;
            }

            // Convention currently is that Providers are always located in an assembly called Odin.BackgroundProcessing.ProviderName
            // If not we will can AssemblyName into the config.
            string providerAssemblyName = $"{Constants.RootNamespace}.{options.Provider}";
            string providerName = $"{Constants.RootNamespace}.{options.Provider}{Constants.ModuleVerb}";

            ResultValue<IBackgroundProcessorServiceInjector> serviceInjectorCreation =
                ClassFactory.TryCreate<IBackgroundProcessorServiceInjector>($"{providerAssemblyName}ServiceInjector", providerAssemblyName);

            if (serviceInjectorCreation.IsSuccess)
            {
                serviceInjectorCreation.Value.TryAddBackgroundProcessor(serviceCollection, configuration,
                    configurationSection, sqlServerConnectionStringFactory);
            }
            else
            {
                string message = $"Unable to load provider {providerName} from {providerAssemblyName}.";
                message += $"This can occur if the {providerAssemblyName} Nuget package reference is missing. {serviceInjectorCreation.MessagesToString()}";

                throw new ApplicationException(message);
            }
        }

        /// <summary>
        /// Adds BackgroundProcessing features (such as Hangfire's dashboard) to the HTTP pipeline.
        /// </summary>
        /// <param name="appServices"></param>
        /// <param name="app"></param>
        public static IApplicationBuilder UseBackgroundProcessing(this IApplicationBuilder app, IServiceProvider appServices)
        {
            Contract.RequiresNotNull(appServices);
            Contract.RequiresNotNull(app);

            BackgroundProcessingOptions options = appServices.GetRequiredService<BackgroundProcessingOptions>();
            if (options.Provider == BackgroundProcessingProviders.Null)
            {
                return app;
            }

            string providerAssemblyName = $"{Constants.RootNamespace}.{options.Provider}";
            ResultValue<IBackgroundProcessorServiceInjector> serviceInjectorCreation =
                ClassFactory.TryCreate<IBackgroundProcessorServiceInjector>($"{providerAssemblyName}ServiceInjector",providerAssemblyName);

            if (serviceInjectorCreation.IsSuccess)
            {
                return serviceInjectorCreation.Value.UseBackgroundProcessing(app, appServices);
            }

            string message = $"Unable to load {Constants.RootNamespace} provider {options.Provider}.";
            if (BackgroundProcessingProviders.HasValue(options.Provider))
            {
                message += $" Ensure that Nuget package {providerAssemblyName} is referenced.";
            }
            else
            {
                message += $" {options.Provider} is not a recognised IBackgroundProcessor provider. Valid providers are {string.Join(" | ", BackgroundProcessingProviders.Values)}";
            }

            throw new ApplicationException(message);
        }
    }
}