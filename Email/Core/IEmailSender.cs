using Odin.System;

namespace Odin.Email
{
    /// <summary>
    /// Provides email sending services
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email
        /// </summary>
        /// <param name="email"></param>
        /// <returns>Success and the Mailgun messageId populated in the Value of the Outcome if available.</returns>
        Task<ResultValue<string>> SendEmail(IEmailMessage email);
        
    }
}