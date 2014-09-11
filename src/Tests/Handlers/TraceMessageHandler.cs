namespace Tests.Handlers
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    using global::Tests.Extensions;

    public class TraceMessageHandler : DelegatingHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TraceMessageHandler"/> class.
        /// </summary>
        public TraceMessageHandler()
            : this(new HttpClientHandler())
        {   
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceMessageHandler" /> class.
        /// </summary>
        /// <param name="innerHandler">The inner handler.</param>
        public TraceMessageHandler(HttpMessageHandler innerHandler)
        {
            this.InnerHandler = innerHandler;
        }

        /// <summary>
        /// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
        /// </summary>
        /// <param name="request">The HTTP request message to send to the server.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>Returns <see cref="T:System.Threading.Tasks.Task`1" />. The task object representing the asynchronous operation.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var startTime = DateTime.Now;

            var requestMessage = string.Format("Request: {0} {1}", request.Method, request.RequestUri);
            Trace.TraceInformation(requestMessage);

            // Try to read out the request content
            if (request.Content != null)
            {
                await request.Content.LoadIntoBufferAsync();

                Trace.TraceInformation("Request Body: {0}", await request.Content.ReadAsStringAsync());
            }

            var response = await base.SendAsync(request, cancellationToken);

            var responseMessage = string.Format(
                "Response: {0} {1} - {{ Time: {2}, Size: {3} }}",
                (int)response.StatusCode,
                response.ReasonPhrase,
                DateTime.Now.Subtract(startTime).AsHumanReadableString(),
                response.Content != null ? response.Content.SizeAsHumanReadableString() : "0");

            // Try to read out the response content
            if (response.Content != null)
            {
                await response.Content.LoadIntoBufferAsync();
            }

            if (!response.IsSuccessStatusCode)
            {
                Trace.TraceError(responseMessage);

                if (response.Content != null)
                {
                    Trace.TraceError(string.Format("Response Body: {0}", await response.Content.ReadAsStringAsync()));
                }
            }
            else
            {
                Trace.TraceInformation(responseMessage);

                if (response.Content != null)
                {
                    Trace.TraceInformation("Response Body: {0}", await response.Content.ReadAsStringAsync());
                }
            }

            return response;
        }
    }
}