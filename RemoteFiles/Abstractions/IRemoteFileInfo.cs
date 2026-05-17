namespace Odin.RemoteFiles;

/// <summary>
/// Basic file information
/// </summary>
public interface IRemoteFileInfo
{

    /// <summary>
    /// Gets the full path of the directory or file.
    /// </summary>
    string FullName { get; }

    /// <summary>
    /// For files, gets the name of the file. For directories, gets the name of the last directory in the hierarchy if a hierarchy exists.
    /// Otherwise, the Name property gets the name of the directory.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Date of file creation on remote system
    /// </summary>
    DateTimeOffset LastWriteTime { get; }
}