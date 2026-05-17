namespace Odin.Email
{
    /// <summary>
    /// Used for To, CC and BCC in EmailAddress
    /// </summary>
    public sealed class EmailAddressCollection : List<EmailAddress>
    {
        /// <summary>
        /// Default constructor for an empty address collection
        /// </summary>
        public EmailAddressCollection()
        {
        }

        /// <summary>
        /// Creates a collection, adding one or more email addresses (separated by comma or semi-colon).
        /// </summary>
        /// <param name="emailAddresses">Separated by a comma (,) or semi-colon (;)</param>
        public EmailAddressCollection(string emailAddresses)
        {
            if (string.IsNullOrWhiteSpace(emailAddresses)) return;

            string[] emails = emailAddresses.Split(new char[] { ',', ';' });
            foreach (string email in emails)
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    AddAddress(email);
                }
            }
        }

        /// <summary>
        /// Adds an EmailAddress to the collection.
        /// If email is empty or null, no EmailAddress is added to the collection.
        /// </summary>
        /// <param name="email">The email address to be added. Can be in format 'DisplayName name at email.com' </param>
        /// <param name="displayName">Ignored if blank</param>
        public void AddAddress(string email, string? displayName = null)
        {
            if (string.IsNullOrWhiteSpace(email)) return;
            if (string.IsNullOrWhiteSpace(displayName))
            {
                Add(new EmailAddress(email));
            }
            else
            {
                Add(new EmailAddress(email, displayName));
            }
        }

        /// <summary>
        /// Adds one or more email addresses (separated by comma or semi-colon) to the collection. 
        /// </summary>
        /// <param name="emailAddresses"></param>
        public void AddAddresses(string emailAddresses)
        {
            EmailAddressCollection addressesToAdd = new EmailAddressCollection(emailAddresses);
            AddRange(addressesToAdd);
        }
    }
}