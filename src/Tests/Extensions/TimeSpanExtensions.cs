namespace Tests.Extensions
{
    using System;

    public static class TimeSpanExtensions
    {
        /// <summary>
        /// Returns the timespan as a human readable string.
        /// </summary>
        /// <param name="ts">The timespan.</param>
        /// <returns>The timespan as a human readable string.</returns>
        public static string AsHumanReadableString(this TimeSpan ts)
        {
            // Larger or equal to 1 second but lesser than 1 minute
            if (ts.TotalMilliseconds >= 1000 && ts.TotalMilliseconds < 60000)
            {
                return string.Format("{0}s", ts.TotalMilliseconds / 1000);
            }

            // Larger or equal to 1 minute but lesser than 1 hour
            if (ts.TotalMilliseconds >= 60000 && ts.TotalMilliseconds < 3600000)
            {
                return string.Format("{0}m", ts.TotalMilliseconds / 1000 / 60);
            }

            // Larger or equal to 1 hour but lesser than 1 day
            if (ts.TotalMilliseconds >= 3600000 && ts.TotalMilliseconds < 86400000)
            {
                return string.Format("{0}h", ts.TotalMilliseconds / 1000 / 60 / 60);
            }

            return string.Format("{0}ms", ts.TotalMilliseconds);
        }
    }
}