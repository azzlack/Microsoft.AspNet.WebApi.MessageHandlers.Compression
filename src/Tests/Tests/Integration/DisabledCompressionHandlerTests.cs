namespace Tests.Tests.Integration
{
    using Extensions;

    using Microsoft.AspNet.WebApi.Extensions.Compression.Server;
    using NUnit.Framework;
    using System;
    using System.Net.Http;
    using System.Net.Http.Extensions.Compression.Client;
    using System.Net.Http.Extensions.Compression.Core;
    using System.Net.Http.Extensions.Compression.Core.Compressors;
    using System.Net.Http.Headers;
    using System.Web.Http;

    [TestFixture]
    public class DisabledCompressionHandlerWithThresholdTests
    {
        private HttpServer server;

        public HttpClient Client { get; set; }

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

            config.MessageHandlers.Insert(0, new ServerCompressionHandler(0, new GZipCompressor(StreamManager.Instance), new DeflateCompressor(StreamManager.Instance))
            {
                IsCompressionEnabled = false
            });

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
        
        [Test]
        public async void GetImage_WhenMessageHandlerIsConfigured_ShouldReturnUncompressedContent()
        {

            var response = await this.Client.GetAsync("http://localhost:55399/api/file/image");

            Console.Write(await response.ToTestString());

            Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "image/png");
            Assert.IsFalse(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsByteArrayAsync();

            Assert.AreEqual(4596, content.Length);
        }
        
        [Test]
        public async void GetImage_WhenAttributeIsConfigured_ShouldReturnCompressedContent()
        {
            var response = await this.Client.GetAsync("http://localhost:55399/api/file/compressedimage");

            Console.Write(await response.ToTestString());

            Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "image/png");
            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsByteArrayAsync();

            Assert.AreEqual(749, response.Content.Headers.ContentLength);
            Assert.AreEqual(4596, content.Length);
        }
    }
}
