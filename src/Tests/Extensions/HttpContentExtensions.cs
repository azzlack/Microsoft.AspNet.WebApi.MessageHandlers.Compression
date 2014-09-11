namespace Tests.Extensions
{
    using System.Net.Http;
    using System.Threading.Tasks;

    public static class HttpContentExtensions
    {
        /// <summary>
        /// Gets the payload size as a human readable string.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The payload size as a human readable string.</returns>
        public static string SizeAsHumanReadableString(this HttpContent content)
        {
            if (content.Headers.ContentLength.HasValue)
            {
                return content.Headers.ContentLength.Value.SizeAsHumanReadableString(LongExtensions.SIType.Byte);
            }

            var tcs = new TaskCompletionSource<byte[]>();
            Task.Run(async () =>
            {
                await content.LoadIntoBufferAsync();

                tcs.SetResult(await content.ReadAsByteArrayAsync());
            }).ConfigureAwait(false);

            return tcs.Task.Result.SizeAsHumanReadableString();
        }
    }
}