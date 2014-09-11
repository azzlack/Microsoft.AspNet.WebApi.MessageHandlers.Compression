namespace Tests.Tests.Integration
{
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http;

    using Microsoft.AspNet.WebApi.MessageHandlers.Compression;
    using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Compressors;

    using NUnit.Framework;

    [TestFixture]
    public class CompressionHandlerWithThresholdTests
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

            config.MessageHandlers.Insert(0, new ServerCompressionHandler(32, new GZipCompressor(), new DeflateCompressor()));

            this.server = new HttpServer(config);
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
        public async void Get_WhenResponseSizeIsLessThanTreshold_ShouldReturnUncompressedContent()
        {
            var client = new HttpClient(new ClientCompressionHandler(this.server, new GZipCompressor(), new DeflateCompressor()));

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            var response = await client.GetAsync("http://localhost:55399/api/test");
            
            var content = await response.Content.ReadAsStringAsync();

            Assert.AreEqual(response.Content.Headers.ContentLength, content.Length);

            Assert.IsFalse(response.Content.Headers.ContentEncoding.Contains("gzip"));
        }
    }
}
