using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Odin.DesignContracts;

namespace Odin.Email;

/// <inheritdoc />
public class Office365ServiceInjector : IEmailSenderServiceInjector
{
    /// <inheritdoc />
    public void TryAddEmailSender(IServiceCollection serviceCollection, IConfigurationSection emailConfigurationSection)
    {
        Contract.RequiresNotNull(emailConfigurationSection);

        EmailSendingOptions emailOptions = new();
        emailConfigurationSection.Bind(emailOptions);

        Office365Options office365Options = new();
        emailConfigurationSection.Bind(EmailSendingProviders.Office365, office365Options);
        office365Options.Validate();

        serviceCollection.AddOdinLoggerWrapper();
        serviceCollection.TryAddSingleton(office365Options);
        serviceCollection.TryAddTransient<IEmailSender, Office365EmailSender>();
        
    }
}