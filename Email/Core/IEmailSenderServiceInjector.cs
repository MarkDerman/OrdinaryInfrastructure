using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Odin.Email
{
    /// <summary>
    /// Sends an email
    /// </summary>
    public interface IEmailSenderServiceInjector
    {
        /// <summary>
        /// Supports injection of specific EmailSender
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="emailSendingConfigurationSection"></param>
        void TryAddEmailSender(IServiceCollection serviceCollection, IConfigurationSection emailSendingConfigurationSection);
    }
}