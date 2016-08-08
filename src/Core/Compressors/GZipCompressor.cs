namespace System.Net.Http.Extensions.Compression.Core.Compressors
{
    using System.IO;
    using System.IO.Compression;
    using System.Net.Http.Extensions.Compression.Core.Interfaces;

    /// <summary>
    /// Compressor for handling <c>gzip</c> encodings.
    /// </summary>
    public class GZipCompressor : BaseCompressor
    {
        /// <summary>Initializes a new instance of the <see cref="GZipCompressor" /> class.</summary>
        public GZipCompressor()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="GZipCompressor" /> class.</summary>
        /// <param name="streamManager">Manager for stream.</param>
        public GZipCompressor(IStreamManager streamManager)
            : base(streamManager)
        {
        }

        /// <summary>
        /// Gets the encoding type.
        /// </summary>
        /// <value>The encoding type.</value>
        public override string EncodingType
        {
            get { return "gzip"; }
        }

        /// <summary>
        /// Creates the compression stream.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <returns>The compressed stream.</returns>
        public override Stream CreateCompressionStream(Stream output)
        {
            return new GZipStream(output, CompressionMode.Compress, true);
        }

        /// <summary>
        /// Creates the decompression stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns>The decompressed stream.</returns>
        public override Stream CreateDecompressionStream(Stream input)
        {
            return new GZipStream(input, CompressionMode.Decompress, true);
        }
    }
}