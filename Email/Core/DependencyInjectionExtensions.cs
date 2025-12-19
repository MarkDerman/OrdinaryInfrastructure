using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Odin.DesignContracts;
using Odin.Email;
using Odin.System;
using Odin.Utility;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Dependency injection extension methods to support EmailSending
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Sets up EmailSending from Configuration, using the EmailSending section by default.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static void AddOdinEmailSending(
            this IServiceCollection serviceCollection, IConfiguration configuration,
            string sectionName = EmailSendingOptions.DefaultConfigurationSectionName)
        {
            IConfigurationSection? section = configuration.GetSection(sectionName);
            if (section.Value == null && !section.GetChildren().Any())
            {
                throw new ApplicationException(
                    $"{nameof(AddOdinEmailSending)}: Configuration section {sectionName} does not exist.");
            }
            serviceCollection.AddOdinEmailSending(section);
        }

        /// <summary>
        /// Sets up EmailSending from the provided ConfigurationSection
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configurationSection"></param>
        /// <returns></returns>
        public static void AddOdinEmailSending(
            this IServiceCollection serviceCollection, IConfigurationSection configurationSection)
        {
            Precondition.RequiresNotNull(configurationSection);

            EmailSendingOptions emailOptions = new EmailSendingOptions();
            configurationSection.Bind(emailOptions);
            Result emailValidationResult = emailOptions.Validate();
            if (!emailValidationResult.IsSuccess)
            {
                throw new ApplicationException(
                    $"Invalid EmailSending configuration. Errors are: {emailValidationResult.MessagesToString()}");
            }

            serviceCollection.TryAddSingleton(emailOptions);

            // Add Sender as per config...
            if (emailOptions.Provider == EmailSendingProviders.Null)
            {
                // Fake sender is built in...
                serviceCollection.TryAddTransient<IEmailSender, NullEmailSender>();
                return;
            }

            string providerAssemblyName = $"{Constants.RootNamespace}.{emailOptions.Provider}";
            ResultValue<IEmailSenderServiceInjector> serviceInjectorCreation =
                ClassFactory.TryCreate<IEmailSenderServiceInjector>($"{providerAssemblyName}ServiceInjector", providerAssemblyName);

            if (serviceInjectorCreation.IsSuccess)
            {
                serviceInjectorCreation.Value.TryAddEmailSender(serviceCollection, configurationSection);
            }
            else
            {
                string message = $"Unable to load EmailSending provider {emailOptions.Provider}.";
                if (EmailSendingProviders.HasValue(emailOptions.Provider))
                {
                    message += $" Ensure Nuget package {providerAssemblyName} is referenced.";
                }
                else
                {
                    message += $" {emailOptions.Provider} is not a recognised IEmailSender provider. Valid Providers are: {string.Join(" | ", EmailSendingProviders.Values)}";
                }

                throw new ApplicationException(message);
            }
        }
    }
}