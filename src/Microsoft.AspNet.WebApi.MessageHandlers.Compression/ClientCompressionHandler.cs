namespace Microsoft.AspNet.WebApi.MessageHandlers.Compression
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Interfaces;
    using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Models;

    /// <summary>
    /// Message handler for handling gzip/deflate requests/responses on a <see cref="HttpClient"/>.
    /// </summary>
    public class ClientCompressionHandler : DelegatingHandler
    {
        /// <summary>
        /// The content size threshold before compressing.
        /// </summary>
        private readonly int contentSizeThreshold;

        /// <summary>
        /// The HTTP content operations
        /// </summary>
        private readonly HttpContentOperations httpContentOperations;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientCompressionHandler" /> class.
        /// </summary>
        /// <param name="compressors">The compressors.</param>
        public ClientCompressionHandler(params ICompressor[] compressors)
        {
            this.Compressors = compressors;
            this.httpContentOperations = new HttpContentOperations();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientCompressionHandler" /> class.
        /// </summary>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="compressors">The compressors.</param>
        public ClientCompressionHandler(int contentSizeThreshold, params ICompressor[] compressors)
            : this(compressors)
        {
            this.contentSizeThreshold = contentSizeThreshold;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="compressors">The compressors.</param>
        public ClientCompressionHandler(HttpMessageHandler innerHandler, params ICompressor[] compressors)
            : this(compressors)
        {
            this.InnerHandler = innerHandler;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="compressors">The compressors.</param>
        public ClientCompressionHandler(HttpMessageHandler innerHandler, int contentSizeThreshold, params ICompressor[] compressors)
            : this(contentSizeThreshold, compressors)
        {
            this.InnerHandler = innerHandler;
        }

        /// <summary>
        /// Gets the compressors.
        /// </summary>
        /// <value>The compressors.</value>
        public ICollection<ICompressor> Compressors { get; private set; }

        /// <summary>
        /// send as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />. The task object representing the asynchronous operation.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Compress uncompressed requests from the client to the server
            if (request.Content != null && request.Headers.AcceptEncoding.Any())
            {
                await this.CompressRequest(request);
            }
            
            var response = await base.SendAsync(request, cancellationToken);

            // Decompress compressed responses to the client from the server
            if (response.Content != null && response.Content.Headers.ContentEncoding.Any())
            {
                await this.DecompressResponse(response);
            }

            return response;
        }

        /// <summary>
        /// Compresses the content.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>An async void.</returns>
        private async Task CompressRequest(HttpRequestMessage request)
        {
            // As per RFC2616.14.3:
            // Ignores encodings with quality == 0
            // If multiple content-codings are acceptable, then the acceptable content-coding with the highest non-zero qvalue is preferred.
            var compressor = (from encoding in request.Headers.AcceptEncoding
                              let quality = encoding.Quality ?? 1.0
                              where quality > 0
                              join c in this.Compressors on encoding.Value.ToLowerInvariant() equals
                                  c.EncodingType.ToLowerInvariant()
                              orderby quality descending
                              select c).FirstOrDefault();

            if (compressor != null)
            {
                // Only compress response if size is larger than threshold (if set)
                if (this.contentSizeThreshold == 0)
                {
                    request.Content = new CompressedContent(request.Content, compressor);   
                }
                else if (this.contentSizeThreshold > 0 && request.Content.Headers.ContentLength >= this.contentSizeThreshold)
                {
                    request.Content = new CompressedContent(request.Content, compressor);
                }
            }
        }

        /// <summary>
        /// Decompresses the response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <returns>An async void.</returns>
        private async Task DecompressResponse(HttpResponseMessage response)
        {
            var encoding = response.Content.Headers.ContentEncoding.FirstOrDefault();

            if (encoding != null) 
            {
                var compressor = this.Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding, StringComparison.OrdinalIgnoreCase));

                if (compressor != null)
                {
                    response.Content = await this.httpContentOperations.DecompressContent(response.Content, compressor);
                }
            }
        }
    }
}