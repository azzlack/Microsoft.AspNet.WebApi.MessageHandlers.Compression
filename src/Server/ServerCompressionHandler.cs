namespace Microsoft.AspNet.WebApi.Extensions.Compression.Server
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Extensions.Compression.Core.Compressors;
    using System.Net.Http.Extensions.Compression.Core.Interfaces;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Message handler for handling gzip/deflate requests/responses.
    /// </summary>
    public class ServerCompressionHandler : BaseServerCompressionHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        public ServerCompressionHandler()
            : base(null, 860, new GZipCompressor(StreamManager.Instance), new DeflateCompressor(StreamManager.Instance))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="compressors">The compressors.</param>
        public ServerCompressionHandler(params ICompressor[] compressors)
            : base(null, 860, null, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="compressors">The compressors.</param>
        public ServerCompressionHandler(int contentSizeThreshold, params ICompressor[] compressors)
            : base(null, contentSizeThreshold, null, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="compressors">The compressors.</param>
        public ServerCompressionHandler(HttpMessageHandler innerHandler, params ICompressor[] compressors)
            : base(innerHandler, 860, null, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="compressors">The compressors.</param>
        public ServerCompressionHandler(HttpMessageHandler innerHandler, int contentSizeThreshold, params ICompressor[] compressors)
            : base(innerHandler, contentSizeThreshold, null, compressors)
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
            : base(innerHandler, contentSizeThreshold, enableCompression, compressors)
        {
        }

        /// <summary>Handles the request.</summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>A Task&lt;HttpResponseMessage&gt;</returns>
        public override Task<HttpRequestMessage> HandleRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return this.HandleDecompression(request, cancellationToken);
        }

        /// <summary>Handles the response.</summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The handled response.</returns>
        public override Task<HttpResponseMessage> HandleResponse(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
        {
            return this.HandleCompression(request, response, cancellationToken);
        }
    }
}
