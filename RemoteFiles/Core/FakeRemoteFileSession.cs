namespace Odin.RemoteFiles
{
    /// <summary>
    /// Fake implementation of IFileOperationsProvider
    /// </summary>
    public sealed class FakeRemoteFileSession : IRemoteFileSession
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="behaviour"></param>
        public FakeRemoteFileSession(
            FakeBehaviour behaviour =
                FakeBehaviour.Return)
        {
            Behaviour = behaviour;
        }

        /// <summary>
        /// Disconnect fake
        /// </summary>
        public void Disconnect()
        {
            BehaveAsFake();
        }

        /// <summary>
        /// Connect fake
        /// </summary>
        public void Connect()
        {
            BehaveAsFake();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textFileContents"></param>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public void UploadFile(string textFileContents, string fullPath)
        {
            BehaveAsFake();
        }

        /// <summary>
        /// DownloadFile
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="output"></param>
        public void DownloadFile(string fileName, in Stream output)
        {
            BehaveAsFake();
        }

        /// <summary>
        /// DownloadTextFile
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string DownloadTextFile(string fileName)
        {
            BehaveAsFake();
            return ("");
        }

        /// <summary>
        /// Changes directory
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="ApplicationException"></exception>
        /// <exception cref="Exception"></exception>
        public void ChangeDirectory(string path)
        {
            BehaveAsFake();
        }

        private void BehaveAsFake()
        {
            switch (Behaviour)
            {
                case FakeBehaviour.Return:
                    return; // Task.FromResult(new SimpleResult<bool>(true));
                case FakeBehaviour.ThrowException:
                    throw new ApplicationException("FakeBackgroundJobProvider throwing an exception");
                default:
                    throw new Exception($"Unknown Behaviour - {Behaviour}");
            }
        }

        /// <summary>
        /// CreateDirectory fake
        /// </summary>
        /// <param name="path"></param>
        public void CreateDirectory(string path)
        {
            BehaveAsFake();
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="filePath"></param>
        public void Delete(string filePath)
        {
            BehaveAsFake();
        }

        /// <summary>
        /// list fake
        /// </summary>
        /// <param name="path">The path to the directory to search under</param>
        /// <param name="searchPattern">Optional search pattern, supporting wildcards (*) and (?).</param>
        /// <returns></returns>
        public IEnumerable<IRemoteFileInfo> GetFiles(string path, string? searchPattern = null)
        {
            switch (Behaviour)
            {
                case FakeBehaviour.Return:
                    return new List<RemoteFileInfo>(); // Task.FromResult(new SimpleResult<bool>(true));
                case FakeBehaviour.ThrowException:
                    throw new ApplicationException("FakeBackgroundJobProvider throwing an exception");
                default:
                    throw new Exception($"Unknown Behaviour - {Behaviour}");
            }
        }

        /// <summary>
        /// Exists fake
        /// </summary>
        /// <param name="path"></param>
        /// <param name="timeoutInSeconds"></param>
        /// <returns></returns>
        public bool Exists(string path, int? timeoutInSeconds = null)
        {
            switch (Behaviour)
            {
                case FakeBehaviour.Return:
                    return true;
                case FakeBehaviour.ThrowException:
                    throw new ApplicationException("FakeBackgroundJobProvider throwing an exception");
                default:
                    throw new Exception($"Unknown Behaviour - {Behaviour}");
            }
        }

        /// <summary>
        /// Behaviour of fake provider
        /// </summary>
        public FakeBehaviour Behaviour { get; set; } = FakeBehaviour.Return;

        /// <summary>
        /// Fake behaviour enum
        /// </summary>
        public enum FakeBehaviour
        {
            /// <summary>
            /// Succeed
            /// </summary>
            Return,

            /// <summary>
            /// Throw an Exception
            /// </summary>
            ThrowException
        }
    }
}