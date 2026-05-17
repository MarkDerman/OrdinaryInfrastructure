namespace Odin.Email
{
    /// <summary>
    /// Default IEmailMessage for .NET
    /// </summary>
    public interface IEmailMessage
    {
        /// <summary>
        /// From email address
        /// </summary>
        EmailAddress? From { get; set; }

        /// <summary>
        /// To email addresses
        /// </summary>
        EmailAddressCollection To { get; set; }

        /// <summary>
        /// CC email addresses
        /// </summary>
        EmailAddressCollection CC { get; set; }

        /// <summary>
        /// BCC email addresses
        /// </summary>
        EmailAddressCollection BCC { get; set; }

        /// <summary>
        /// ReplyTo email addresses
        /// </summary>
        EmailAddress? ReplyTo { get; set; }

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
        /// Email subject
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// Email body
        /// </summary>
        string Body { get; set; }

        /// <summary>
        /// Email priority
        /// </summary>
        Priority Priority { get; set; }

        /// <summary>
        /// Email tags (not supported by all EmailSending providers)
        /// </summary>
        List<string> Tags { get; set; }

        /// <summary>
        /// Flags the body as HTML
        /// </summary>
        bool IsHtml { get; set; }

        /// <summary>
        /// Email headers
        /// </summary>
        Dictionary<string, string> Headers { get; set; }
    }
}