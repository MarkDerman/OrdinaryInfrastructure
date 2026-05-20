namespace Odin.Emailing
{
    /// <summary>
    /// Default IEmailMessage for .NET
    /// </summary>
    public interface IEmailMessage
    {
        /// <summary>
        /// Address from whom the email originates. This is optional, as it can also be set at email sending time
        /// from a default in configuration.
        /// </summary>
        EmailAddress? From { get; set; }
        
        /// <summary>
        /// Address of the person sending the email (on behalf of From)
        /// </summary>
        EmailAddress? Sender { get; set; }

        /// <summary>
        /// List of recipient email addresses
        /// </summary>
        EmailAddressCollection To { get; set; }

        /// <summary>
        /// List of CC email addresses
        /// </summary>
        EmailAddressCollection CC { get; set; }

        /// <summary>
        /// List BCC email addresses
        /// </summary>
        EmailAddressCollection BCC { get; set; }

        /// <summary>
        /// ReplyTo email addresses
        /// </summary>
        EmailAddressCollection ReplyTo { get; set; }

        /// <summary>
        /// Attachments
        /// </summary>
        List<Attachment> Attachments { get; set; }

        /// <summary>
        /// Adds an attachment 
        /// </summary>
        /// <param name="attachment"></param>
        void Attach(Attachment attachment);

        /// <summary>
        /// Email subject (required)
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// Html body
        /// </summary>
        string? HtmlBody { get; set; }

        /// <summary>
        /// Text body
        /// </summary>
        string? TextBody { get; set; }

        /// <summary>
        /// Email priority. Defaults to Normal.
        /// </summary>
        Priority Priority { get; set; }

        /// <summary>
        /// Email tags (not supported by all providers)
        /// </summary>
        List<string> Tags { get; set; }

        /// <summary>
        /// Returns true if the HtmlBody is set.
        /// </summary>
        bool IsHtml { get; }

        /// <summary>
        /// Email headers
        /// </summary>
        Dictionary<string, string> Headers { get; set; }
        
        /// <summary>
        /// Structured data often used for tracking or internal purposes
        /// </summary>
        Dictionary<string, object?> Metadata { get; set; }
        
        // /// <summary>
        // /// Many providers support scheduling emails for future delivery.
        // /// </summary>
        // DateTime? SendAtUtc { get; set; }
    }
}