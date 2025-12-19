using Odin.System;

namespace Odin.Email
{
    /// <summary>
    /// The IEmailSender providers supported
    /// </summary>
    public class EmailSendingProviders : StringEnum<EmailSendingProviders>
    {
        /// <summary>
        /// Null provider for testing. Does nothing.
        /// </summary>
        public const string Null = "Null";
        
        /// <summary>
        /// Mailgun V3 API
        /// </summary>
        public const string Mailgun = "Mailgun";

        /// <summary>
        /// Office365 via Microsoft Graph API.
        /// </summary>
        public const string Office365 = "Office365";

    }
}