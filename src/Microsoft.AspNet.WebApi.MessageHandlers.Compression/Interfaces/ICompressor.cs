namespace Microsoft.AspNet.WebApi.MessageHandlers.Compression.Interfaces
{
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for stream compressors.
    /// </summary>
    public interface ICompressor
    {
        /// <summary>
        /// Gets the encoding type.
        /// </summary>
        /// <value>The encoding type.</value>
        string EncodingType { get; }

        /// <summary>
        /// Compresses the specified source stream onto the destination stream.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <returns>An async void.</returns>
        Task Compress(Stream source, Stream destination);

        /// <summary>
        /// Decompresses the specified source stream onto the destination stream.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <returns>An async void.</returns>
        Task Decompress(Stream source, Stream destination);
    }
}