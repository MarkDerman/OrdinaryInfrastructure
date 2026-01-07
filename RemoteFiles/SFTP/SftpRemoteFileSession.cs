using System.Text;
using System.Text.RegularExpressions;
using Odin.DesignContracts;
using Renci.SshNet;
using Renci.SshNet.Sftp;


namespace Odin.RemoteFiles
{
    /// <summary>
    /// SSH FTP IFileOperationsProvider implementation
    /// </summary>
    public sealed class SftpRemoteFileSession : IRemoteFileSession
    {
        private readonly SftpConnectionSettings _connectionInfo;
        private SftpClient? _client;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="connectionInfo"></param>
        public SftpRemoteFileSession(SftpConnectionSettings connectionInfo)
        {
            Contract.Requires(connectionInfo!=null!);
            _connectionInfo = connectionInfo!;
        }

        /// <summary>
        /// Connects to the SFTP server. Throws an Exception if failure occurs...
        /// </summary>
        public void Connect()
        {
            EnsureConnected();
        }

        private void EnsureConnected(int? timeoutInSeconds = null)
        {
            if (_client == null)
            {
                List<AuthenticationMethod> authMethods = new List<AuthenticationMethod>();
                if (_connectionInfo.HasPrivateKey())
                {
                    PrivateKeyFile pk;
                    if (_connectionInfo.HasPrivateKeyPassphrase())
                    {
                        pk = new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(_connectionInfo.PrivateKey)),
                            _connectionInfo.PrivateKeyPassphrase);
                    }
                    else
                    {
                        pk = new PrivateKeyFile(new MemoryStream(Encoding.ASCII.GetBytes(_connectionInfo.PrivateKey)));
                    }

                    PrivateKeyFile[] keyFiles = { pk };
                    authMethods.Add(new PrivateKeyAuthenticationMethod(_connectionInfo.UserName, keyFiles));
                }

                if (_connectionInfo.HasUsernameAndPassword())
                {
                    authMethods.Add(
                        new PasswordAuthenticationMethod(_connectionInfo.UserName, _connectionInfo.Password));
                }

                if (!authMethods.Any())
                {
                    throw new ApplicationException(
                        "Either Username and Password must be specified, or UserName, PrivateKey and PrivateKeyPhrase, or both.");
                }
                ConnectionInfo connInfo = new ConnectionInfo(_connectionInfo.Host,
                    _connectionInfo.Port,
                    _connectionInfo.UserName, authMethods.ToArray());
                if (timeoutInSeconds != null) connInfo.Timeout = new TimeSpan(0, 0, timeoutInSeconds.Value); //
                _client = new SftpClient(connInfo);
                _client.Connect();
            }
            else if (!_client.IsConnected)
            {
                _client.Connect();
            }
        }

        /// <summary>
        /// Disconnects the client if currently connected
        /// </summary>
        public void Disconnect()
        {
            if (_client != null && _client.IsConnected)
            {
                _client.Disconnect();
            }
        }

        /// <summary>
        /// Uploads a file via Sftp to the remote server 
        /// </summary>
        /// <param name="textFileContents"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public void UploadFile(string textFileContents, string fileName)
        {
            Contract.Requires<ArgumentNullException>(textFileContents != null, nameof(textFileContents));
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fileName), nameof(fileName));

            EnsureConnected();
            MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(textFileContents));
            _client!.BufferSize = 4096;
            _client!.UploadFile(stream, fileName);
        }

        /// <summary>
        /// Downloads a file via Sftp to the remote server 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public void DownloadFile(string fileName, in Stream output)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fileName), nameof(fileName));

            EnsureConnected();
            _client!.BufferSize = 4096;
            _client!.DownloadFile(fileName, output);
        }

        /// <summary>
        /// DownloadTextFile
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string DownloadTextFile(string fileName)
        {
            string contents;
            using (MemoryStream memStream = new MemoryStream(4096))
            {
                DownloadFile(fileName, memStream);
                // StreamReader reader = new StreamReader(memStream);
                // contents = reader.ReadToEnd();
                // memStream.
                contents = Encoding.ASCII.GetString(memStream.ToArray());
            }

            return contents;
        }

        /// <summary>
        /// Changes directory
        /// </summary>
        /// <param name="path"></param>
        public void ChangeDirectory(string path)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path), nameof(path));
            EnsureConnected();
            _client.ChangeDirectory(path);
        }

        /// <summary>
        /// CreateDirectory
        /// </summary>
        /// <param name="path"></param>
        public void CreateDirectory(string path)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path), nameof(path));
            EnsureConnected();
            _client!.CreateDirectory(path);
        }

        /// <summary>
        /// Deletes a file or directory
        /// </summary>
        /// <param name="filePath"></param>
        public void Delete(string filePath)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(filePath), nameof(filePath));
            EnsureConnected();
            _client!.DeleteFile(filePath);
        }

        /// <summary>
        /// Returns the files (IRemoteFileInfo) that match the specified search pattern in the specified directory
        /// </summary>
        /// <param name="path">The path to the directory to search under</param>
        /// <param name="searchPattern">Optional search pattern for the file name under the specified path. Supports wildcards (*) and (?).</param>
        /// <returns></returns>
        public IEnumerable<IRemoteFileInfo> GetFiles(string path, string? searchPattern = null)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path), nameof(path));
            Contract.Requires(!(path!.Contains('*') || path.Contains('?')));
            EnsureConnected();
            //return results
            IEnumerable<ISftpFile> files = _client.ListDirectory(path);
            if (!string.IsNullOrWhiteSpace(searchPattern))
            {
                if (searchPattern.Contains('?') || searchPattern.Contains('*'))
                {
                    return files.OrderBy(file => file.LastWriteTime)
                        .Where(file => !file.IsDirectory && IsMatch(file.Name, searchPattern))
                        .Select(p => new RemoteFileInfo(p.FullName, p.Name, p.LastWriteTimeUtc));
                }
                return files.Where(p => string.Equals(p.Name, searchPattern))
                    .Select(p => new RemoteFileInfo(p.FullName, p.Name, p.LastWriteTimeUtc));
            }
            return files.Select(c => new RemoteFileInfo(c.FullName, c.Name, c.LastWriteTimeUtc));
        }

        /// <summary>
        /// Checks for file or directory existence. 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="timeoutInSeconds"></param>
        /// <returns></returns>
        public bool Exists(string path, int? timeoutInSeconds = null)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(path), nameof(path));
            EnsureConnected(timeoutInSeconds);
            return _client.Exists(path);
        }

        private static bool IsMatch(string fileName, string pattern)
        {
            // Replace '*' with '.*' and '?' with '.' in the pattern
            string regexPattern = "^" + Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".") + "$";
            return Regex.IsMatch(fileName, regexPattern, RegexOptions.IgnoreCase);
        }
    }
}