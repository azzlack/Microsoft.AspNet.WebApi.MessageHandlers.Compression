using System.Diagnostics;

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

    using Microsoft.IO;

    /// <summary>
    /// Base message handler for handling gzip/deflate requests/responses.
    /// </summary>
    public abstract class BaseServerCompressionHandler : DelegatingHandler
    {

        /// <summary>
        /// true to attempt to marshal the continuation back to the original context captured; default is false.
        /// </summary>
        private readonly bool continueOnCapturedContext;

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

        /// <summary>Manager for recyclable streams.</summary>
        private readonly IStreamManager streamManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="compressors">The compressors.</param>
        protected BaseServerCompressionHandler(params ICompressor[] compressors)
            : this(null, 860, null, false, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="continueOnCapturedContext">ConfigureAwait value.</param>
        /// <param name="compressors">The compressors.</param>
        protected BaseServerCompressionHandler(bool continueOnCapturedContext, params ICompressor[] compressors)
            : this(null, 860, null, continueOnCapturedContext, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="compressors">The compressors.</param>
        protected BaseServerCompressionHandler(int contentSizeThreshold, params ICompressor[] compressors)
            : this(null, contentSizeThreshold, null, false, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="continueOnCapturedContext">ConfigureAwait value.</param>
        /// <param name="compressors">The compressors.</param>
        protected BaseServerCompressionHandler(int contentSizeThreshold, bool continueOnCapturedContext, params ICompressor[] compressors)
            : this(null, contentSizeThreshold, null, continueOnCapturedContext, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="compressors">The compressors.</param>
        protected BaseServerCompressionHandler(HttpMessageHandler innerHandler, params ICompressor[] compressors)
            : this(innerHandler, 860, null, false, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="continueOnCapturedContext">ConfigureAwait value.</param>
        /// <param name="compressors">The compressors.</param>
        protected BaseServerCompressionHandler(HttpMessageHandler innerHandler, bool continueOnCapturedContex, params ICompressor[] compressors)
            : this(innerHandler, 860, null, continueOnCapturedContex, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="compressors">The compressors.</param>
        protected BaseServerCompressionHandler(HttpMessageHandler innerHandler, int contentSizeThreshold, params ICompressor[] compressors)
            : this(innerHandler, contentSizeThreshold, null, false, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="continueOnCapturedContext">ConfigureAwait value.</param>
        /// <param name="compressors">The compressors.</param>
        protected BaseServerCompressionHandler(HttpMessageHandler innerHandler, int contentSizeThreshold,
            bool continueOnCapturedContex, params ICompressor[] compressors)
            : this(innerHandler, contentSizeThreshold, null, continueOnCapturedContex, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="enableCompression">Custom delegate to enable or disable compression.</param>
        /// <param name="compressors">The compressors.</param>
        protected BaseServerCompressionHandler(HttpMessageHandler innerHandler, int contentSizeThreshold,
            Predicate<HttpRequestMessage> enableCompression, params ICompressor[] compressors)
            : this(innerHandler, contentSizeThreshold, enableCompression, false, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="enableCompression">Custom delegate to enable or disable compression.</param>
        /// /// <param name="continueOnCapturedContext">ConfigureAwait value.</param>
        /// <param name="compressors">The compressors.</param>
        protected BaseServerCompressionHandler(HttpMessageHandler innerHandler, int contentSizeThreshold,
            Predicate<HttpRequestMessage> enableCompression, bool continueOnCapturedContext,
            params ICompressor[] compressors)
        {
            if (innerHandler != null)
            {
                this.InnerHandler = innerHandler;
            }

            this.continueOnCapturedContext = continueOnCapturedContext;
            this.Compressors = compressors;
            this.contentSizeThreshold = contentSizeThreshold;
            this.httpContentOperations = new HttpContentOperations();
            this.streamManager = new RecyclableStreamManager();

            this.enableCompression = enableCompression ?? (x =>
            {
                // If request does not accept gzip or deflate, return false
                if (x.Headers.AcceptEncoding.All(y => y.Value != "gzip" && y.Value != "deflate" && y.Value!="*"))
                {
                    return false;
                }

                // If compression has been explicitly disabled, return false
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

        /// <summary>Handles the request.</summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The handled request.</returns>
        public abstract Task<HttpRequestMessage> HandleRequest(HttpRequestMessage request, CancellationToken cancellationToken);

        /// <summary>Handles the response.</summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The handled response.</returns>
        public abstract Task<HttpResponseMessage> HandleResponse(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken);

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task`1"/>. The task object representing the asynchronous operation.
        /// </returns>
        /// <param name="request">The HTTP request message to send to the server.</param><param name="cancellationToken">A cancellation token to cancel operation.</param><exception cref="T:System.ArgumentNullException">The <paramref name="request"/> was null.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request = await this.HandleRequest(request, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(continueOnCapturedContext);

            return await this.HandleResponse(request, response, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }

        /// <summary>Handles the decompression if applicable.</summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The response, decompressed if applicable.</returns>
        public virtual async Task<HttpRequestMessage> HandleDecompression(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Decompress compressed requests to the server
            if (request.Content != null && request.Content.Headers.ContentEncoding.Any(y => y == "gzip" || y == "deflate"))
            {
                await this.DecompressRequest(request);
            }

            return request;
        }

        /// <summary>Handles the compression if applicable.</summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The response, compressed if applicable.</returns>
        public virtual async Task<HttpResponseMessage> HandleCompression(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
        {
            // sometimes in a cancelled call response is null
            if (response == null) return response;
            
            // Check if response should be compressed
            // NOTE: This must be done _after_ the response has been created, otherwise the EnableCompression property is not set
            var process = this.enableCompression(request);

            // Compress content if it should processed
            if (process && response.Content != null)
            {
                try
                {
                    // Buffer content for further processing if content length is not known
                    if (!this.ContentLengthKnown(response))
                    {
                        await response.Content.LoadIntoBufferAsync();
                    }

                    await this.CompressResponse(request, response);
                }
                catch (ObjectDisposedException x)
                {
                    Trace.TraceError($"Could not compress request, as response.Content had already been disposed: {x.Message}");
                }
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
            // Manage the Accept-Encoding: * by using the first compressor
            ICompressor compressor = null;

            if ( request.Headers.AcceptEncoding.FirstOrDefault(w => w.Value == "*") != null )
            {
                compressor=Compressors.FirstOrDefault();
            }
            else
            {
                compressor = (from encoding in request.Headers.AcceptEncoding
                              let quality = encoding.Quality ?? 1.0
                              where quality > 0
                              join c in this.Compressors on encoding.Value.ToLowerInvariant() equals c.EncodingType.ToLowerInvariant()
                              orderby quality descending
                              select c).FirstOrDefault();
            }
            if (compressor != null)
            {
                try
                {
                    // Only compress response if not already compressed
                    if (response.Content?.Headers.ContentEncoding != null 
                        && response.Content.Headers.ContentEncoding.Contains(compressor.EncodingType))
                    {
                        return;
                    }

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
                    request.Content = await this.httpContentOperations.DecompressContent(request.Content, compressor).ConfigureAwait(continueOnCapturedContext);
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Unable to decompress request using compressor '{0}'", compressor.GetType()), ex);
                }
            }
        }

        /// <summary>Checks if the response content length is known.</summary>
        /// <param name="response">The response.</param>
        /// <returns>true if it is known, false if it it is not.</returns>
        private bool ContentLengthKnown(HttpResponseMessage response)
        {
            if (response?.Content == null)
            {
                return false;
            }

            if (response.Content is StringContent)
            {
                return true;
            }

            if (response.Content is ByteArrayContent)
            {
                return true;
            }

            return false;
        }
    }
}
