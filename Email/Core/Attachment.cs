using System.Text.Json.Serialization;

namespace Odin.Email
{
    /// <summary>
    /// Email attachment
    /// </summary>
    public sealed record Attachment
    {

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="contentType"></param>
        public Attachment(string fileName, Stream data, string contentType)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
            ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
            ArgumentNullException.ThrowIfNull(data);
            FileName = fileName;
            Data = data;
            ContentType = contentType;
        }

        // /// <summary>
        // /// IsInline
        // /// </summary>
        // public bool IsInline { get; set; }

        /// <summary>
        /// Filename of the attachment
        /// </summary>
        public string FileName { get; init; }

        /// <summary>
        /// Attachment data as a stream
        /// </summary>
        [JsonIgnore]
        public Stream Data { get; set; }

        /// <summary>
        /// ContentType
        /// </summary>
        public string ContentType { get; set; }

    }
}