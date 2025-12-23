using Odin.DesignContracts;

namespace Odin.RemoteFiles;

/// <summary>
/// Basic file information
/// </summary>
public sealed class RemoteFileInfo : IRemoteFileInfo
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="fullName"></param>
    /// <param name="name"></param>
    /// <param name="lastWriteTimeUtc"></param>
    public RemoteFileInfo(string fullName, string name, DateTime lastWriteTimeUtc)
    {
        Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(name), nameof(name));
        Contract.Requires<ArgumentNullException>(!string.IsNullOrWhiteSpace(fullName), nameof(fullName));
        FullName = fullName;
        Name = name;
        // Use overload for DateTimeOffset set from DateTime
        LastWriteTime = DateTime.SpecifyKind(lastWriteTimeUtc, DateTimeKind.Utc);
    }
        
    /// <summary>
    /// Gets the full path of the directory or file.
    /// </summary>
    public string FullName { get; private set; }

    /// <summary>
    /// For files, gets the name of the file. For directories, gets the name of the last directory in the hierarchy if a hierarchy exists.
    /// Otherwise, the Name property gets the name of the directory.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// File creation date
    /// </summary>
    public DateTimeOffset LastWriteTime { get; set; }
}