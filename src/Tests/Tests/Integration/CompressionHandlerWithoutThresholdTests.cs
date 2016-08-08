namespace Tests.Tests.Integration
{
    using global::Tests.Tests.Common;
    using Microsoft.AspNet.WebApi.Extensions.Compression.Server;
    using NUnit.Framework;
    using System.Net.Http;
    using System.Net.Http.Extensions.Compression.Client;
    using System.Net.Http.Extensions.Compression.Core;
    using System.Net.Http.Extensions.Compression.Core.Compressors;
    using System.Net.Http.Headers;
    using System.Web.Http;

    [TestFixture]
    public class CompressionHandlerWithoutThresholdTests : TestFixture
    {
        private HttpServer server;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            config.MessageHandlers.Insert(0, new ServerCompressionHandler(0, new GZipCompressor(StreamManager.Instance), new DeflateCompressor(StreamManager.Instance)));

            this.server = new HttpServer(config);
        }

        [SetUp]
        public void SetUp()
        {
            var client = new HttpClient(new ClientCompressionHandler(this.server, 0, new GZipCompressor(), new DeflateCompressor()));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            this.Client = client;
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (this.server != null)
            {
                this.server.Dispose();
            }
        }
    }
}
