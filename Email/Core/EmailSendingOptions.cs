using Odin.DesignContracts;
using Odin.System;


namespace Odin.Email
{
    /// <summary>
    /// Email settings for loading from configuration 
    /// </summary>
    public record EmailSendingOptions
    {
        private string _provider = EmailSendingProviders.Mailgun;

        /// <summary>
        /// Name of the default EmailSending configuration section in application IConfiguration
        /// </summary>
        public const string DefaultConfigurationSectionName = "EmailSending";
        
        /// <summary>
        /// Default from emailAddress for the generic IEmailSender in DI
        /// </summary>
        public string? DefaultFromAddress { get; set; }

        /// <summary>
        /// Default from name for the generic IEmailSender in DI
        /// </summary>
        public string? DefaultFromName { get; set; }
        
        /// <summary>
        /// Text to prefix the Subject of every email sent.
        /// Useful for marking emails sent from different testing environments. 
        /// </summary>
        public string? SubjectPrefix { get; set; }
        
        /// <summary>
        /// Text to postfix the Subject of every email sent.
        /// Useful for marking emails sent from different testing environments. 
        /// </summary>
        public string? SubjectPostfix { get;  set; }

        /// <summary>
        /// Default tags (i.e. Office365 Categories) for the generic IEmailSender in DI
        /// Does nothing when using the Mailgun sender.
        /// </summary>
        public List<string>? DefaultTags { get; set; }

        /// <summary>
        /// Email sending provider - Mailgun, Office365 and Fake are only ones at this stage.
        /// </summary>
        public string Provider
        {
            get => _provider;
            init
            {
                Precondition.Requires(!string.IsNullOrWhiteSpace(value));
                // Ensure MailgunEmailSender is changed to Mailgun for backwards compatibility
                _provider = value.Replace("EmailSender", "", StringComparison.OrdinalIgnoreCase);   
            }
        }


        /// <summary>
        /// Validates the settings instance
        /// </summary>
        /// <returns></returns>
        public Result Validate()
        {
            List<string> errors = new List<string>();
            if (!EmailSendingProviders.HasValue(Provider))
            {
                errors.Add(
                    $"{nameof(Provider)}: {Provider} is not supported. Supported providers are {string.Join(" | ", EmailSendingProviders.Values)}");
            }
            return new Result(!errors.Any(), errors);
        }
        
    }
}
