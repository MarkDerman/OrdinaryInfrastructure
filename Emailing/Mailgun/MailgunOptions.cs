using Odin.System;

namespace Odin.Emailing
{
    /// <summary>
    /// MailgunSettings for loading from configuration 
    /// </summary>
    public sealed class MailgunOptions
    {
        /// <summary>
        /// 'Mailgun'
        /// Also default EmailSending configuration section name for Mailgun
        /// </summary>
        public const string MailgunName = "Mailgun";

        /// <summary>
        /// Europe
        /// </summary>
        public const string RegionEU = "EU";

        /// <summary>
        /// USA
        /// </summary>
        public const string RegionUSA = "USA";

        /// <summary>
        /// API key
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Sender domain
        /// </summary>
        public string? Domain { get; set; }

        /// <summary>
        /// EU or USA for Mailgun
        /// </summary>
        public string Region { get; set; } = RegionEU;

        /// <summary>
        /// Validates the settings instance
        /// </summary>
        /// <returns></returns>
        public Result IsConfigurationValid()
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                errors.Add("ApiKey is missing");
            }
            else if (ApiKey.Length < 40)
            {
                errors.Add("ApiKey is not a valid Mailgun key");
            }
            if (string.IsNullOrWhiteSpace(Domain)) errors.Add("Domain is not specified");
            if (Region != RegionEU && Region != RegionUSA) errors.Add("Region must be EU or USA");
            return new Result(!errors.Any(), errors);
        }

    }

}