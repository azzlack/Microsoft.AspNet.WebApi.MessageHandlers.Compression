namespace Microsoft.AspNet.WebApi.Extensions.Compression.Server
{
    using System.IO;
    using System.Net.Http.Extensions.Compression.Core.Interfaces;

    using Microsoft.IO;

    /// <summary>Manager for streams.</summary>
    public class StreamManager : IStreamManager
    {
        private readonly RecyclableMemoryStreamManager recyclableMemoryStreamManager;

        /// <summary>Initializes a new instance of the <see cref="StreamManager" /> class.</summary>
        public StreamManager()
        {
            this.recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public static IStreamManager Instance { get; } = new StreamManager();

        public Stream GetStream(string tag = null)
        {
            return !string.IsNullOrEmpty(tag)
                       ? this.recyclableMemoryStreamManager.GetStream(tag)
                       : this.recyclableMemoryStreamManager.GetStream();
        }
    }
}