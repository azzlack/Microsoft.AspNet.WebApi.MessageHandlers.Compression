namespace Microsoft.AspNet.WebApi.MessageHandlers.Compression
{
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
            using (compressedContent)
            {
                var decompressedContentStream = new MemoryStream();

                await compressor.Decompress(await compressedContent.ReadAsStreamAsync(), decompressedContentStream);

                // Set position back to 0 so it can be read again
                decompressedContentStream.Position = 0;

                var decompressedContent = new StreamContent(decompressedContentStream);

                // Copy content headers so we know what got sent back
                this.CopyHeaders(compressedContent, decompressedContent);

                return decompressedContent;
            }
        }

        /// <summary>
        /// Copies the HTTP headers onto the new response.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        public void CopyHeaders(HttpContent input, HttpContent output)
        {
            // Allow: {methods}
            foreach (var allow in input.Headers.Allow)
            {
                output.Headers.Allow.Add(allow);
            }

            // Content-Disposition: {disposition-type}; {disposition-param}
            output.Headers.ContentDisposition = input.Headers.ContentDisposition;

            // Content-Encoding: {content-encodings}
            foreach (var encoding in input.Headers.ContentEncoding)
            {
                output.Headers.ContentEncoding.Add(encoding);
            }

            // Content-Language: {languages}
            foreach (var language in input.Headers.ContentLanguage)
            {
                output.Headers.ContentLanguage.Add(language);
            }

            // Content-Length: {size}
            output.Headers.ContentLength = input.Headers.ContentLength;

            // Content-Location: {uri}
            output.Headers.ContentLocation = input.Headers.ContentLocation;

            // Content-MD5: {md5-digest}
            output.Headers.ContentMD5 = input.Headers.ContentMD5;

            // Content-Range: {range}
            output.Headers.ContentRange = input.Headers.ContentRange;

            // Content-Type: {media-types}
            output.Headers.ContentType = input.Headers.ContentType;

            // Expires: {http-date}
            output.Headers.Expires = input.Headers.Expires;

            // LastModified: {http-date}
            output.Headers.LastModified = input.Headers.LastModified;
        }
    }
}
