namespace Tests.Tests.Facade
{
    using global::Tests.Extensions;
    using global::Tests.Handlers;
    using global::Tests.Models;
    using global::Tests.Tests.Common;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.WebApi.Extensions.Compression.Server.Owin;
    using Microsoft.Owin;
    using Microsoft.Owin.Security.Cookies;
    using Microsoft.Owin.Testing;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Owin;
    using System;
    using System.Net.Http;
    using System.Net.Http.Extensions.Compression.Client;
    using System.Net.Http.Extensions.Compression.Core.Compressors;
    using System.Net.Http.Headers;
    using System.Text;
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

        [Test]
        public async void Post_WhenHeaderIsModifiedOnServer_ShouldReturnModifiedHeader()
        {
            var loginModel = new LoginModel("Test", "Test", "http://localhost:55399/api/test");
            var response = await this.Client.PostAsync("http://localhost:55399/api/test/login", new StringContent(JsonConvert.SerializeObject(loginModel), Encoding.UTF8, "application/json"));

            Console.Write(await response.ToTestString());

            Assert.AreEqual(loginModel.ReturnUrl, response.Headers.Location.ToString());
        }
        public class OwinStartup
        {
            public void Configuration(IAppBuilder appBuilder)
            {
                var config = new HttpConfiguration();
                config.MapHttpAttributeRoutes();
                config.Routes.MapHttpRoute("DefaultApi", "api/{controller}/{id}", new { id = RouteParameter.Optional });

                // Add compression message handler
                config.MessageHandlers.Insert(0, new OwinServerCompressionHandler(0, new GZipCompressor(), new DeflateCompressor()));

                appBuilder.UseCookieAuthentication(
                    new CookieAuthenticationOptions()
                    {
                        AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                        LoginPath = new PathString("/api/test/login")
                    });

                appBuilder.UseWebApi(config);
            }
        }
    }
}