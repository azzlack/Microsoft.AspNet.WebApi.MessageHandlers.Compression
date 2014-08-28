namespace Tests
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class TraceMessageHandler : DelegatingHandler
    {
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

            var task = await base.SendAsync(request, cancellationToken);

            if (request.Content != null)
            {
                Trace.TraceInformation("Request Body: {0}", await request.Content.ReadAsStringAsync());
            }

            var response = string.Format(
                "Response: {0} {1} - {{ Time: {2}, Size: {3} }}",
                (int)task.StatusCode,
                task.ReasonPhrase,
                DateTime.Now.Subtract(startTime),
                task.Content != null ? (await task.Content.ReadAsByteArrayAsync()).Length : 0);

            if (!task.IsSuccessStatusCode)
            {
                Trace.TraceError(requestMessage);
                Trace.TraceError(response);
            }
            else
            {
                Trace.TraceInformation(response);
            }

            if (task.Content != null)
            {
                var responseBody = string.Format("Response Body: {0}", await task.Content.ReadAsStringAsync());

                if (!task.IsSuccessStatusCode)
                {
                    Trace.TraceError(responseBody);
                }
                else
                {
                    Trace.TraceInformation(responseBody);
                }
            }

            return task;
        }
    }
}