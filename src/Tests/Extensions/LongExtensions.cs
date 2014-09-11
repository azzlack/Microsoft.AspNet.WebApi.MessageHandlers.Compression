namespace Tests.Extensions
{
    using System;

    internal static class LongExtensions
    {
        /// <summary>
        /// Gets the size as a human readable string.
        /// </summary>
        /// <param name="input">The byte array.</param>
        /// <returns>The size as a human readable string.</returns>
        public static string SizeAsHumanReadableString(this long input, SIType siType)
        {
            if (siType == SIType.Byte)
            {
                // Larger or equal to 1KB but lesser than 1MB
                if (input >= 1024 && input < 1048576)
                {
                    return string.Format("{0}KB", input / 1024);
                }

                // Larger or equal to 1MB but lesser than 1GB
                if (input >= 1048576 && input < 1073741824)
                {
                    return string.Format("{0}MB", input / 1024 / 1024);
                }

                // Larger or equal to 1Gt but lesser than 1TB
                if (input >= 1073741824 && input < 1099511627776)
                {
                    return string.Format("{0}GB", input / 1024 / 1024 / 1024);
                }

                return string.Format("{0}bytes", input);
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Enumeration for the Internation System of Units (SI).
        /// </summary>
        public enum SIType
        {
            /// <summary>
            /// The byte unit type (B, kB, MB, GB, TB, etc.)
            /// </summary>
            Byte
        }
    }
}