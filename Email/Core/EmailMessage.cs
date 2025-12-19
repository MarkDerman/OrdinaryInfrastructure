using Odin.DesignContracts;

namespace Odin.Email
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
            IsHtml = false;
            Body = "";
            Subject = "";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toEmailAddress"></param>
        /// <param name="fromEmailAddress"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="isHtml"></param>
        public EmailMessage(string toEmailAddress, string fromEmailAddress, string? subject, string? body,
            bool isHtml = false)
        {
            Precondition.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(toEmailAddress),
                $"{nameof(toEmailAddress)} is required");
            Precondition.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(fromEmailAddress),
                $"{nameof(fromEmailAddress)} is required");
            if (string.IsNullOrWhiteSpace(subject))
            {
                Subject = "";
            }
            else
            {
                Subject = subject;
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                Body = "";
            }
            else
            {
                Body = body;
            }

            Priority = Priority.Normal;
            IsHtml = isHtml;
            To.AddAddresses(toEmailAddress);
            From = new EmailAddress(fromEmailAddress);
        }

        /// <summary>
        /// From email address
        /// </summary>
        public EmailAddress? From { get; set; }

        /// <summary>
        /// To email addresses
        /// </summary>
        public EmailAddressCollection To
        {
            get
            {
                if (_to == null)
                {
                    _to = new EmailAddressCollection();
                }

                return _to;
            }
            set => _to = value;
        }

        private EmailAddressCollection _to = null!;

        /// <summary>
        /// CC email addresses
        /// </summary>
        public EmailAddressCollection CC
        {
            get
            {
                if (_cc == null)
                {
                    _cc = new EmailAddressCollection();
                }

                return _cc;
            }
            set => _cc = value;
        }

        private EmailAddressCollection _cc = null!;

        /// <summary>
        /// BCC email addresses
        /// </summary>
        public EmailAddressCollection BCC
        {
            get
            {
                if (_bcc == null)
                {
                    _bcc = new EmailAddressCollection();
                }

                return _bcc;
            }
            set => _bcc = value;
        }

        private EmailAddressCollection _bcc = null!;

        /// <summary>
        /// ReplyTo email addresses
        /// </summary>
        public EmailAddress? ReplyTo { get; set; }

        /// <summary>
        /// Attachments
        /// </summary>
        public List<Attachment> Attachments
        {
            get
            {
                if (_attachments == null)
                {
                    _attachments = new List<Attachment>();
                }

                return _attachments;
            }
            set => _attachments = value;
        }

        private List<Attachment> _attachments = null!;

        /// <summary>
        /// Email subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Email body
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Email priority
        /// </summary>
        public Priority Priority { get; set; }

        /// <summary>
        /// Email tags (not supported by all EmailSending providers)
        /// </summary>
        public List<string> Tags
        {
            get
            {
                if (_tags == null)
                {
                    _tags = new List<string>();
                }

                return _tags;
            }
            set => _tags = value;
        }

        private List<string> _tags = null!;

        /// <summary>
        /// Flags the body as HTML. Default is false.
        /// </summary>
        public bool IsHtml { get; set; }

        /// <summary>
        /// Email headers
        /// </summary>
        public Dictionary<string, string> Headers
        {
            get
            {
                if (_headers == null)
                {
                    _headers = new Dictionary<string, string>();
                }

                return _headers;
            }
            set => _headers = value;
        }

        private Dictionary<string, string> _headers = null!;

        /// <summary>
        /// Attaches an attachments to the email
        /// </summary>
        /// <param name="attachment"></param>
        public void Attach(Attachment attachment)
        {
            Precondition.RequiresNotNull(attachment);
            Attachments.Add(attachment);
        }
    }
}