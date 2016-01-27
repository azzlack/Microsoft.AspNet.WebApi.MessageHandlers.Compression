namespace System.Net.Http.Extensions.Compression.Core
{
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Extensions.Compression.Core.Extensions;
    using System.Net.Http.Extensions.Compression.Core.Interfaces;
    using System.Threading.Tasks;

    /// <summary>
    /// Helper methods for operating on <see cref="HttpContent"/> instances.
    /// </summary>
    public class HttpContentOperations
    {
        /// <summary>
        /// Decompresses the compressed HTTP content.
        /// </summary>
        /// <param name="compressedContent">The compressed HTTP content.</param>
        /// <param name="compressor">The compressor.</param>
        /// <returns>The decompressed content.</returns>
        public async Task<HttpContent> DecompressContent(HttpContent compressedContent, ICompressor compressor)
        {
            var decompressedContentStream = new MemoryStream();

            // Decompress buffered content
            using (var ms = new MemoryStream(await compressedContent.ReadAsByteArrayAsync()))
            {
                await compressor.Decompress(ms, decompressedContentStream).ConfigureAwait(false);
            }

            // Set position back to 0 so it can be read again
            decompressedContentStream.Position = 0;

            var decompressedContent = new StreamContent(decompressedContentStream);

            // Copy content headers so we know what got sent back
            compressedContent.Headers.CopyTo(decompressedContent.Headers);

            return decompressedContent;
        }
    }
}
