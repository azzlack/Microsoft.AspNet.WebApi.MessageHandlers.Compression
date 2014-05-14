namespace Microsoft.AspNet.WebApi.MessageHandlers.Compression.Compressors
{
    using System.IO;
    using System.IO.Compression;

    /// <summary>
    /// Compressor for handling <c>deflate</c> encodings.
    /// </summary>
    public class DeflateCompressor : BaseCompressor
    {
        /// <summary>
        /// Gets the encoding type.
        /// </summary>
        /// <value>The encoding type.</value>
        public override string EncodingType
        {
            get { return "deflate"; }
        }

        /// <summary>
        /// Creates the compression stream.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <returns>The compressed stream.</returns>
        public override Stream CreateCompressionStream(Stream output)
        {
            return new DeflateStream(output, CompressionMode.Compress, true);
        }

        /// <summary>
        /// Creates the decompression stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns>The decompressed stream.</returns>
        public override Stream CreateDecompressionStream(Stream input)
        {
            return new DeflateStream(input, CompressionMode.Decompress, true);
        }
    }
}