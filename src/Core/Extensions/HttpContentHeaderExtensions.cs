namespace System.Net.Http.Extensions.Compression.Core.Extensions
{
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http.Headers;

    public static class HttpContentHeaderExtensions
    {
        /// <summary>Copies all headers from the source to the target.</summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="handleContentLength">true to handle content length.</param>
        /// <param name="handleContentEncoding">true to handle content encoding.</param>
        /// <param name="handleChangedValues">true to handle changed values.</param>
        public static void CopyTo(
            this HttpContentHeaders source,
            HttpContentHeaders target,
            bool handleContentLength = true,
            bool handleContentEncoding = true,
            bool handleChangedValues = false)
        {
            // Copy all other headers unless they have been modified and we want to preserve that
            foreach (var header in source)
            {
                try
                {
                    if (!handleContentLength && header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!handleContentEncoding && header.Key.Equals("Content-Encoding", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (!handleChangedValues)
                    {
                        // If values have changed, dont update it
                        if (target.Contains(header.Key))
                        {
                            if (target.GetValues(header.Key).Any(targetValue => header.Value.Any(originalValue => originalValue != targetValue)))
                            {
                                continue;
                            }
                        }
                    }

                    target.Add(header.Key, header.Value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }
    }
}