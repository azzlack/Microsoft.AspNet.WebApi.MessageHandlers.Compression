namespace Microsoft.AspNet.WebApi.MessageHandlers.Compression
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Interfaces;

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
            this.CopyHeaders(compressedContent, decompressedContent);

            return decompressedContent;
        }

        /// <summary>
        /// Copies the HTTP headers onto the new response.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        public void CopyHeaders(HttpContent input, HttpContent output)
        {
            // All other headers
            foreach (var header in input.Headers)
            {
                try
                {
                    output.Headers.Add(header.Key, header.Value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }
    }
}
