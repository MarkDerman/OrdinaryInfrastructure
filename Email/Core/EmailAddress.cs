using Odin.DesignContracts;

namespace Odin.Email
{
    /// <summary>
    /// For To, CC or Bcc in IEmailMessage
    /// </summary>
    public sealed class EmailAddress
    {
        /// <summary>
        /// Display name (optional)
        /// </summary>
        public string? DisplayName { get;  }

        /// <summary>
        /// Email address
        /// </summary>
        public string Address { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="emailAddress">Required</param>
        /// <param name="displayName"></param>
        public EmailAddress(string emailAddress, string? displayName = null)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(emailAddress), $"{nameof(emailAddress)} is required");
            if (string.IsNullOrWhiteSpace(displayName))
            {
                DisplayName = null;
                // Parse emailAddress looking for < and > and split out address and display name.
                int openIndex = emailAddress.IndexOf('<');
                int closedIndex = emailAddress.IndexOf('>');
                if (openIndex >= 0 && closedIndex > openIndex+1)
                {
                    // OK looks like this is display name and email in the string.
                    Address = emailAddress.Substring(openIndex + 1, closedIndex - openIndex - 1).Trim();
                    if (openIndex > 0)
                    {
                        DisplayName = emailAddress.Substring(0, openIndex).Trim();
                    }
                }
                else
                {
                    Address = emailAddress.Trim();
                }
            }
            else
            {
                // Assume email is email only, and display name is given.
                Address = emailAddress.Trim();
                DisplayName = displayName.Trim();
            }
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <returns></returns>
        public override string ToString() =>
            DisplayName != null ? DisplayName + " <" + Address + ">" : Address;

        /// <summary>
        /// GetHashCode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj == null || !GetType().Equals(obj.GetType()))
                return false;
            EmailAddress emailAddress = (EmailAddress) obj;
            return Address == emailAddress.Address && DisplayName == emailAddress.DisplayName;
        }
    }
}