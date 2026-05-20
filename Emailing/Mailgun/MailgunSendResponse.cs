namespace Odin.Emailing
{
    /// <summary>
    /// Returned by Mailgun after email send
    /// </summary>
    public sealed class MailgunSendResponse
    {
        /// <summary>
        /// Mailgun message Id
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        public string? Message { get; set; }
    }
}