namespace Microsoft.AspNet.WebApi.MessageHandlers.Compression
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Interfaces;

    /// <summary>
    /// Message handler for 
    /// </summary>
    public class DecompressionHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecompressionHandler" /> class.
        /// </summary>
        /// <param name="compressors">The compressors.</param>
        public DecompressionHandler(params ICompressor[] compressors)
        {
            this.Compressors = compressors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler which is responsible for processing the HTTP response messages.</param>
        /// <param name="compressors">The compressors.</param>
        public DecompressionHandler(HttpMessageHandler innerHandler, params ICompressor[] compressors)
        {
            this.InnerHandler = innerHandler;
            this.Compressors = compressors;
        }

        /// <summary>
        /// Gets the compressors
        /// </summary>
        public ICollection<ICompressor> Compressors { get; private set; }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1"/>. The task object representing the asynchronous operation.
        /// </returns>
        /// <param name="request">The HTTP request message to send to the server.</param><param name="cancellationToken">A cancellation token to cancel operation.</param><exception cref="T:System.ArgumentNullException">The <paramref name="request"/> was null.</exception>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.Content.Headers.ContentEncoding.Any() && response.Content != null)
            {
                var encoding = response.Content.Headers.ContentEncoding.First();

                var compressor = this.Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding, StringComparison.OrdinalIgnoreCase));

                if (compressor != null)
                {
                    response.Content = await DecompressContentAsync(response.Content, compressor).ConfigureAwait(false);
                }
            }

            return response;
        }

        /// <summary>
        /// Decompresses the compressed HTTP content.
        /// </summary>
        /// <param name="compressedContent">The compressed HTTP content.</param>
        /// <param name="compressor">The compressor.</param>
        /// <returns>The decompressed content.</returns>
        private async Task<HttpContent> DecompressContentAsync(HttpContent compressedContent, ICompressor compressor)
        {
            using (compressedContent)
            {
                var decompressedContentStream = new MemoryStream();

                await compressor.Decompress(await compressedContent.ReadAsStreamAsync(), decompressedContentStream).ConfigureAwait(false);

                // Set position back to 0 so it can be read again
                decompressedContentStream.Position = 0;

                var decompressedContent = new StreamContent(decompressedContentStream);

                // Copy content headers so we know what got sent back
                this.CopyHeaders(compressedContent, decompressedContent);

                return decompressedContent;
            }
        }

        private void CopyHeaders(HttpContent input, HttpContent output)
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