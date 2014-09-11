namespace Tests.Extensions
{
    internal static class ByteArrayExtensions
    {
        /// <summary>
        /// Gets the size as a human readable string.
        /// </summary>
        /// <param name="s">The byte array.</param>
        /// <returns>The size as a human readable string..</returns>
        public static string SizeAsHumanReadableString(this byte[] s)
        {
            // Larger or equal to 1KB but lesser than 1MB
            if (s.Length >= 1024 && s.Length < 1048576)
            {
                return string.Format("{0}KB", s.Length / 1024);
            }

            // Larger or equal to 1MB but lesser than 1GB
            if (s.Length >= 1048576 && s.Length < 1073741824)
            {
                return string.Format("{0}MB", s.Length / 1024 / 1024);
            }

            // Larger or equal to 1Gt but lesser than 1TB
            if (s.Length >= 1073741824)
            {
                return string.Format("{0}GB", s.Length / 1024 / 1024 / 1024);
            }

            return string.Format("{0}bytes", s.Length);
        }
    }
}