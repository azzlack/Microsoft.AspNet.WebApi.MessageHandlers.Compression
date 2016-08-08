namespace System.Net.Http.Extensions.Compression.Core
{
    using System.IO;
    using System.Net.Http.Extensions.Compression.Core.Interfaces;

    /// <summary>Manager for streams.</summary>
    public class StreamManager : IStreamManager
    {
        public static IStreamManager Instance { get; } = new StreamManager();

        public Stream GetStream(string tag = null)
        {
            return new MemoryStream();
        }
    }
}