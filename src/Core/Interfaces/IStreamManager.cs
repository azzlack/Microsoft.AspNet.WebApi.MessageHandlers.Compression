namespace System.Net.Http.Extensions.Compression.Core.Interfaces
{
    using System.IO;

    public interface IStreamManager
    {
        /// <summary>Gets a stream.</summary>
        /// <param name="tag">(Optional) the stream name.</param>
        /// <returns>The stream.</returns>
        Stream GetStream(string tag = null);
    }
}