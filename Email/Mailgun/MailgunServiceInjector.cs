using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Odin.DesignContracts;
using Odin.System;

namespace Odin.Email
{
    /// <summary>
    /// Adds MailgunEmailSender to DI
    /// </summary>
    public class MailgunServiceInjector : IEmailSenderServiceInjector
    {
        /// <inheritdoc />
        public void TryAddEmailSender(IServiceCollection serviceCollection,
            IConfigurationSection emailConfigurationSection)
        {
            Contract.RequiresNotNull(emailConfigurationSection);

            MailgunOptions mailGunSenderSettings = new MailgunOptions();
            emailConfigurationSection.Bind(MailgunOptions.MailgunName,
                mailGunSenderSettings);
            Result validationResult = mailGunSenderSettings.IsConfigurationValid();
            if (!validationResult.IsSuccess)
            {
                throw new ApplicationException($"Invalid {nameof(MailgunOptions)}: {validationResult.MessagesToString()}");
            }
            serviceCollection.AddOdinLoggerWrapper();
            serviceCollection.TryAddSingleton(mailGunSenderSettings);
            serviceCollection.TryAddTransient<IEmailSender, MailgunEmailSender>();
        }
    }
}