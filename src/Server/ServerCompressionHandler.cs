namespace Microsoft.AspNet.WebApi.Extensions.Compression.Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Extensions.Compression.Core;
    using System.Net.Http.Extensions.Compression.Core.Interfaces;
    using System.Net.Http.Extensions.Compression.Core.Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Message handler for handling gzip/deflate requests/responses.
    /// </summary>
    public class ServerCompressionHandler : DelegatingHandler
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
        /// Custom delegate to enable or disable compression.
        /// </summary>
        private readonly Predicate<HttpRequestMessage> enableCompression;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="compressors">The compressors.</param>
        public ServerCompressionHandler(params ICompressor[] compressors)
            : this(null, 860, null, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="compressors">The compressors.</param>
        public ServerCompressionHandler(int contentSizeThreshold, params ICompressor[] compressors)
            : this(null, contentSizeThreshold, null, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="compressors">The compressors.</param>
        public ServerCompressionHandler(HttpMessageHandler innerHandler, params ICompressor[] compressors)
            : this(innerHandler, 860, null, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="compressors">The compressors.</param>
        public ServerCompressionHandler(HttpMessageHandler innerHandler, int contentSizeThreshold, params ICompressor[] compressors)
            : this(innerHandler, contentSizeThreshold, null, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="enableCompression">Custom delegate to enable or disable compression.</param>
        /// <param name="compressors">The compressors.</param>
        public ServerCompressionHandler(HttpMessageHandler innerHandler, int contentSizeThreshold, Predicate<HttpRequestMessage> enableCompression, params ICompressor[] compressors)
        {
            if (innerHandler != null)
            {
                this.InnerHandler = innerHandler;
            }

            this.Compressors = compressors;
            this.contentSizeThreshold = contentSizeThreshold;
            this.httpContentOperations = new HttpContentOperations();

            this.enableCompression = enableCompression ?? (x =>
                {
                    if (x.Headers.AcceptEncoding.All(y => y.Value != "gzip" && y.Value != "deflate"))
                    {
                        return false;
                    }

                    if (x.Properties.ContainsKey("compression:Enable"))
                    {
                        bool enable;
                        bool.TryParse(x.Properties["compression:Enable"].ToString(), out enable);

                        return enable;
                    }

                    return true;
                });
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
            // Decompress compressed requests to the server
            if (request.Content != null && request.Content.Headers.ContentEncoding.Any())
            {
                await this.DecompressRequest(request);
            }

            // Perform original action
            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // Check if response should be compressed
            // NOTE: This must be done _after_ the response has been created, otherwise the EnableCompression property is not set
            var process = this.enableCompression(request);

            // Compress content if it should processed
            if (process && response.Content != null)
            {
                // Buffer content for further processing.
                // NOTE: This is needed to properly calculate Content-Length
                await response.Content.LoadIntoBufferAsync();

                await this.CompressResponse(request, response);
            }

            return response;
        }

        /// <summary>
        /// Compresses the content.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <returns>An async void.</returns>
        private async Task CompressResponse(HttpRequestMessage request, HttpResponseMessage response)
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
                try
                {
                    // Only compress response if size is larger than treshold (if set)
                    if (this.contentSizeThreshold == 0)
                    {
                        response.Content = new CompressedContent(response.Content, compressor);
                    }
                    else if (this.contentSizeThreshold > 0 && response.Content.Headers.ContentLength >= this.contentSizeThreshold)
                    {
                        response.Content = new CompressedContent(response.Content, compressor);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Unable to compress response using compressor '{0}'", compressor.GetType()), ex);
                }
            }
        }

        /// <summary>
        /// Decompresses the request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>An async void.</returns>
        private async Task DecompressRequest(HttpRequestMessage request)
        {
            var encoding = request.Content.Headers.ContentEncoding.First();

            var compressor = this.Compressors.FirstOrDefault(c => c.EncodingType.Equals(encoding, StringComparison.OrdinalIgnoreCase));

            if (compressor != null)
            {
                try
                {
                    request.Content = await this.httpContentOperations.DecompressContent(request.Content, compressor).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Unable to decompress request using compressor '{0}'", compressor.GetType()), ex);
                }
            }
        }
    }
}
