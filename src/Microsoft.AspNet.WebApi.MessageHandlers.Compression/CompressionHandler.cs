namespace Microsoft.AspNet.WebApi.MessageHandlers.Compression
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Interfaces;
    using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Models;

    /// <summary>
    /// Message handler for handling gzip/deflate requests/responses.
    /// </summary>
    public class CompressionHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompressionHandler" /> class.
        /// </summary>
        /// <param name="compressors">The compressors.</param>
        public CompressionHandler(params ICompressor[] compressors)
        {
            this.Compressors = compressors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressionHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        /// <param name="compressors">The compressors.</param>
        public CompressionHandler(HttpMessageHandler innerHandler, params ICompressor[] compressors)
        {
            this.InnerHandler = innerHandler;
            this.Compressors = compressors;
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
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (request.Headers.AcceptEncoding.Any() && response.Content != null)
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
                    response.Content = new CompressedContent(response.Content, compressor);
                }
            }

            return response;
        }
    }
}