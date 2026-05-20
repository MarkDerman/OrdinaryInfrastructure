namespace Odin.Emailing
{
    /// <summary>
    /// Default IEmailMessage for .NET
    /// </summary>
    public sealed class EmailMessage : IEmailMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EmailMessage()
        {
            Priority = Priority.Normal;
            TextBody = "";
            Subject = "";
        }

        /// <summary>
        /// Simple constructor for a basic email.
        /// </summary>
        /// <param name="recipients">Comma or semi-colon separated list of email addresses.</param>
        /// <param name="from">Required</param>
        /// <param name="subject">Required</param>
        /// <param name="body">Required</param>
        /// <param name="isBodyHtml">Default is false.</param>
        public EmailMessage(string recipients, string from, string subject, 
            string body, bool isBodyHtml = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(recipients);
            ArgumentException.ThrowIfNullOrWhiteSpace(from);
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);
            ArgumentException.ThrowIfNullOrWhiteSpace(body);
            From = new EmailAddress(from);
            To.AddAddresses(recipients);
            Subject = subject;
            if (isBodyHtml)
            {
                HtmlBody = body;
            }
            else
            {
                TextBody = body;
            }
            Priority = Priority.Normal;
        }

        /// <summary>
        /// Address from whom the email originates
        /// </summary>
        public EmailAddress? From { get; set; }
        
        /// <summary>
        /// Address of the person sending the email (on behalf of From)
        /// </summary>
        public EmailAddress? Sender { get; set; }

        /// <summary>
        /// To email addresses
        /// </summary>
        public EmailAddressCollection To
        {
            get => field ??= [];
            set;
        }

        /// <summary>
        /// CC email addresses
        /// </summary>
        public EmailAddressCollection CC
        {
            get => field ??= [];
            set;
        }

        /// <summary>
        /// BCC email addresses
        /// </summary>
        public EmailAddressCollection BCC
        {
            get => field ??= [];
            set;
        }

        /// <summary>
        /// ReplyTo email addresses
        /// </summary>
        public EmailAddressCollection ReplyTo 
        {
            get => field ??= [];
            set;
        }
        
        /// <summary>
        /// Email subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Html body
        /// </summary>
        public string? HtmlBody { get; set; }

        /// <summary>
        /// Html body
        /// </summary>
        public string? TextBody { get; set; }
        
        /// <summary>
        /// Attachments
        /// </summary>
        public List<Attachment> Attachments
        {
            get => field ??= [];
            set;
        }

        /// <summary>
        /// Attaches an attachments to the email
        /// </summary>
        /// <param name="attachment"></param>
        public void Attach(Attachment attachment)
        {
            ArgumentNullException.ThrowIfNull(attachment);
            Attachments.Add(attachment);
        }
        
        /// <summary>
        /// Email priority
        /// </summary>
        public Priority Priority { get; set; }

        /// <summary>
        /// Email tags (not supported by all EmailSending providers)
        /// </summary>
        public List<string> Tags
        {
            get => field ??= [];
            set;
        }

        /// <summary>
        /// Returns true if HtmlBody is set.
        /// </summary>
        public bool IsHtml 
        {
            get => HtmlBody != null;
        }

        /// <summary>
        /// Email headers
        /// </summary>
        public Dictionary<string, string> Headers
        {
            get => field ??= new Dictionary<string, string>();
            set;
        }

        /// <summary>
        /// Structured data often used for tracking or internal purposes
        /// </summary>
        public Dictionary<string, object?> Metadata { get; set; }
    }
}