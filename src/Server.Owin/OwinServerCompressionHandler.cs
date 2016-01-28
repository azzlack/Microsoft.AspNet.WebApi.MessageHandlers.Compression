namespace Microsoft.AspNet.WebApi.Extensions.Compression.Server.Owin
{
    using Microsoft.AspNet.WebApi.Extensions.Compression.Server;
    using Microsoft.Owin;
    using System;
    using System.Net.Http;
    using System.Net.Http.Extensions.Compression.Core.Interfaces;
    using System.Threading;
    using System.Threading.Tasks;

    public class OwinServerCompressionHandler : ServerCompressionHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OwinServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="compressors">The compressors.</param>
        public OwinServerCompressionHandler(params ICompressor[] compressors)
            : base(null, 860, null, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="compressors">The compressors.</param>
        public OwinServerCompressionHandler(int contentSizeThreshold, params ICompressor[] compressors)
            : base(null, contentSizeThreshold, null, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="compressors">The compressors.</param>
        public OwinServerCompressionHandler(HttpMessageHandler innerHandler, params ICompressor[] compressors)
            : base(innerHandler, 860, null, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="compressors">The compressors.</param>
        public OwinServerCompressionHandler(HttpMessageHandler innerHandler, int contentSizeThreshold, params ICompressor[] compressors)
            : base(innerHandler, contentSizeThreshold, null, compressors)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OwinServerCompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="contentSizeThreshold">The content size threshold before compressing.</param>
        /// <param name="enableCompression">Custom delegate to enable or disable compression.</param>
        /// <param name="compressors">The compressors.</param>
        public OwinServerCompressionHandler(HttpMessageHandler innerHandler, int contentSizeThreshold, Predicate<HttpRequestMessage> enableCompression, params ICompressor[] compressors)
            : base(innerHandler, contentSizeThreshold, enableCompression, compressors)
        {
        }

        /// <summary>Handles the request.</summary>
        /// <param name="request">The request.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>A Task&lt;HttpResponseMessage&gt;</returns>
        public override Task<HttpRequestMessage> HandleRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.GetOwinContext().Response.OnSendingHeaders(
                response =>
                    {
                        ((IOwinResponse)response).Environment["HeadersWritten"] = true;
                    },
                request.GetOwinContext().Response);

            return base.HandleRequest(request, cancellationToken);
        }

        /// <summary>Handles the response.</summary>
        /// <param name="request">The request.</param>
        /// <param name="response">The response.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The handled response.</returns>
        public override async Task<HttpResponseMessage> HandleResponse(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken)
        {
            // Check if headers are already written, and skip processing if they are
            if (request.GetOwinContext().Response.Environment.ContainsKey("HeadersWritten"))
            {
                bool written;
                bool.TryParse(request.GetOwinContext().Response.Environment["HeadersWritten"].ToString(), out written);

                if (written)
                {
                    return response;
                }
            }

            return await base.HandleResponse(request, response, cancellationToken);
        }
    }
}