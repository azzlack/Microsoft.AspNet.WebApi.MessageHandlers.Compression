namespace System.Net.Http.Extensions.Compression.Core.Compressors
{
    using System.IO;
    using System.Net.Http.Extensions.Compression.Core.Interfaces;
    using System.Threading.Tasks;

    /// <summary>
    /// Base compressor for compressing streams.
    /// </summary>
    /// <remarks>
    /// Based on the work by: 
    ///     Ben Foster (http://benfoster.io/blog/aspnet-web-api-compression)
    ///     Kiran Challa (http://blogs.msdn.com/b/kiranchalla/archive/2012/09/04/handling-compression-accept-encoding-sample.aspx)
    /// </remarks>
    public abstract class BaseCompressor : ICompressor
    {
        /// <summary>Manager for stream.</summary>
        private readonly IStreamManager streamManager;

        /// <summary>Initializes a new instance of the <see cref="BaseCompressor" /> class.</summary>
        protected BaseCompressor()
        {
            this.streamManager = StreamManager.Instance;
        }

        /// <summary>Initializes a new instance of the <see cref="BaseCompressor" /> class.</summary>
        /// <param name="streamManager">The stream manager.</param>
        protected BaseCompressor(IStreamManager streamManager)
        {
            this.streamManager = streamManager;
        }

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
        public virtual async Task<long> Compress(Stream source, Stream destination)
        {
            using (var mem = this.streamManager.GetStream())
            {
                using (var gzip = this.CreateCompressionStream(mem))
                {
                    await source.CopyToAsync(gzip);
                }

                mem.Position = 0;

                var compressed = new byte[mem.Length];
                await mem.ReadAsync(compressed, 0, compressed.Length);

                var outStream = new MemoryStream(compressed);
                await outStream.CopyToAsync(destination);

                return mem.Length;
            }
        }

        /// <summary>
        /// Decompresses the specified source stream onto the destination stream.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destination">The destination.</param>
        /// <returns>An async void.</returns>
        public virtual async Task Decompress(Stream source, Stream destination)
        {
            var decompressed = this.CreateDecompressionStream(source);

            await this.Pump(decompressed, destination);

            decompressed.Dispose();
        }

        /// <summary>
        /// Copies the specified input stream onto the output stream.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <returns>An async void.</returns>
        protected virtual async Task Pump(Stream input, Stream output)
        {
            await input.CopyToAsync(output);
        }
    }
}
