namespace Microsoft.AspNet.WebApi.MessageHandlers.Compression.Compressors
{
    using System.IO;
    using System.Threading.Tasks;

    using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Interfaces;

    /// <summary>
    /// Base compressor for compressing streams.
    /// </summary>
    public abstract class BaseCompressor : ICompressor
    {
        /// <summary>
        /// Gets the encoding type.
        /// </summary>
        /// <value>The encoding type.</value>
        public abstract string EncodingType { get; }

        /// <summary>
        /// Creates the compression stream.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <returns>The compressed stream.</returns>
        public abstract Stream CreateCompressionStream(Stream output);

        /// <summary>
        /// Creates the decompression stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns>The decompressed stream.</returns>
        public abstract Stream CreateDecompressionStream(Stream input);

        /// <summary>
        /// Compresses the specified source stream onto the destination stream.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <returns>An async void.</returns>
        public virtual Task Compress(Stream source, Stream destination)
        {
            var compressed = this.CreateCompressionStream(destination);

            return this.Pump(source, compressed).ContinueWith(task => compressed.Dispose());
        }

        /// <summary>
        /// Decompresses the specified source stream onto the destination stream.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <returns>An async void.</returns>
        public virtual Task Decompress(Stream source, Stream destination)
        {
            var decompressed = this.CreateDecompressionStream(source);

            return this.Pump(decompressed, destination).ContinueWith(task => decompressed.Dispose());
        }

        /// <summary>
        /// Copies the specified input stream onto the output stream.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <returns>An async void.</returns>
        protected virtual Task Pump(Stream input, Stream output)
        {
            return input.CopyToAsync(output);
        }
    }
}
