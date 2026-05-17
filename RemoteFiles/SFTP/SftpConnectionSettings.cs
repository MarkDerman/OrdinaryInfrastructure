using Odin.System;

namespace Odin.RemoteFiles
{
    /// <summary>
    /// Connection info for Sftp connections
    /// </summary>
    public sealed record SftpConnectionSettings
    {
        /// <summary>
        /// HostName \ IP (no sftp:// at the beginning)
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// Port number (22 by default)
        /// </summary>
        public int Port { get; set; } = 22;

        /// <summary>
        /// UserName 
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// Password (optional)
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// PrivateKey  (optional)
        /// </summary>
        public string? PrivateKey { get; set; }

        /// <summary>
        /// PrivateKeyPassphrase (optional)
        /// </summary>
        public string? PrivateKeyPassphrase { get; set; }

        /// <summary>
        /// Returns true if PrivateKey and PrivateKeyPassword are both specified.
        /// </summary>
        /// <returns></returns>
        public bool HasPrivateKey()
        {
            return !string.IsNullOrWhiteSpace(PrivateKey);
        }

        /// <summary>
        /// Returns true if a PrivateKeyPassphrase exists
        /// </summary>
        /// <returns></returns>
        public bool HasPrivateKeyPassphrase()
        {
            return !string.IsNullOrWhiteSpace(PrivateKeyPassphrase);
        }

        /// <summary>
        /// Validation
        /// </summary>
        /// <returns></returns>
        public Result IsConfigurationValid()
        {
            List<string> errors = new List<string>();
            if (string.IsNullOrWhiteSpace(Host)) errors.Add("SFTP Host is not specified");
            if (Port <= 0) errors.Add("SFTP Port is not specified");
            return new Result(!errors.Any(), errors);
        }

        /// <summary>
        /// Returns true if both username and password have been set
        /// </summary>
        /// <returns></returns>
        public bool HasUsernameAndPassword()
        {
            return !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password);
        }
    }
}