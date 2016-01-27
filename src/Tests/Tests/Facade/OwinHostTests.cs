namespace Tests.Tests.Facade
{
    using global::Tests.Handlers;
    using global::Tests.Tests.Common;
    using Microsoft.AspNet.WebApi.Extensions.Compression.Server;
    using Microsoft.Owin.Testing;
    using NUnit.Framework;
    using Owin;
    using System;
    using System.Net.Http;
    using System.Net.Http.Extensions.Compression.Client;
    using System.Net.Http.Extensions.Compression.Core.Compressors;
    using System.Net.Http.Headers;
    using System.Web.Http;

    [TestFixture]
    public class OwinHostTests : TestFixture
    {
        private TestServer server;

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            this.server = TestServer.Create<OwinStartup>();
        }

        [SetUp]
        public void SetUp()
        {
            var client = new HttpClient(new TraceMessageHandler(new ClientCompressionHandler(this.server.Handler, new GZipCompressor(), new DeflateCompressor())))
            {
                BaseAddress = new Uri("http://localhost:55399")
            };

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            this.Client = client;
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            this.server.Dispose();
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