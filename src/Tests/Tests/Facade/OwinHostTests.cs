namespace Tests.Tests.Facade
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http;

    using global::Tests.Handlers;
    using global::Tests.Tests.Common;

    using Microsoft.AspNet.WebApi.MessageHandlers.Compression;
    using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Compressors;
    using Microsoft.Owin.Testing;

    using NUnit.Framework;

    using Owin;

    [TestFixture]
    public class OwinHostTests
    {
        private TestServer server;

        private TestFixture testFixture;

        [TestFixtureSetUp]
        public void SetUp()
        {
            this.server = TestServer.Create<OwinStartup>();

            var client = new HttpClient(new TraceMessageHandler(new ClientCompressionHandler(this.server.Handler, new GZipCompressor(), new DeflateCompressor())))
                {
                    BaseAddress = new Uri("http://localhost:55399")
                };

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            this.testFixture = new TestFixture(client);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            this.server.Dispose();
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
        public async void GetImage_WhenAttributeIsConfigured_ShouldReturnUncompressedContent()
        {
            await this.testFixture.GetImage_WhenAttributeIsConfigured_ShouldReturnUncompressedContent();
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

    public class OwinStartup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });

            // Add compression message handler
            config.MessageHandlers.Insert(0, new ServerCompressionHandler(0, new GZipCompressor(), new DeflateCompressor()));

            appBuilder.UseWebApi(config);
        }
    }
}