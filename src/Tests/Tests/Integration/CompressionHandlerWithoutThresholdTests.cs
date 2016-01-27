namespace Tests.Tests.Integration
{
    using System.Net.Http;
    using System.Net.Http.Extensions.Compression.Client;
    using System.Net.Http.Extensions.Compression.Core.Compressors;
    using System.Net.Http.Headers;
    using System.Web.Http;

    using global::Tests.Tests.Common;

    using Microsoft.AspNet.WebApi.Extensions.Compression.Server;

    using NUnit.Framework;

    [TestFixture]
    public class CompressionHandlerWithoutThresholdTests
    {
        private HttpServer server;

        private TestFixture testFixture;

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

            config.MessageHandlers.Insert(0, new ServerCompressionHandler(0, new GZipCompressor(), new DeflateCompressor()));

            this.server = new HttpServer(config);

            var client = new HttpClient(new ClientCompressionHandler(this.server, 0, new GZipCompressor(), new DeflateCompressor()));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            this.testFixture = new TestFixture(client);
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (this.server != null) 
            {
                this.server.Dispose();
            }
        }

        [Test]
        public async void Get_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent()
        {
            await this.testFixture.Get_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent();
        }

        [Test]
        public async void GetImage_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent()
        {
            await this.testFixture.GetImage_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent();
        }

        [Test]
        public async void GetPdf_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent()
        {
            await this.testFixture.GetPdf_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent();
        }
        
        [TestCase("1")]
        [TestCase("10")]
        public async void GetSpecific_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string id)
        {
            await this.testFixture.GetSpecific_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(id);
        }

        [TestCase("Content1")]
        [TestCase("Content10")]
        [TestCase("Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10")]
        public async void Post_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string body)
        {
            await this.testFixture.Post_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(body);
        }

        [TestCase("Content1")]
        [TestCase("Content10")]
        [TestCase("Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10")]
        public async void Post_WhenNestedMessageHandlerIsConfigured_ShouldReturnCompressedContent(string body)
        {
            await this.testFixture.Post_WhenNestedMessageHandlerIsConfigured_ShouldReturnCompressedContent(body);
        }

        [TestCase("1", "Content1")]
        [TestCase("2", "Content10")]
        public async void Put_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string id, string body)
        {
            await this.testFixture.Put_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(id, body);
        }

        [TestCase("1")]
        [TestCase("2")]
        public async void Delete_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string id)
        {
            await this.testFixture.Delete_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(id);
        }
    }
}
