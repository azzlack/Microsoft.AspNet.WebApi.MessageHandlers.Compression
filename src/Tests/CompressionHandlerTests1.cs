namespace Tests
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Web.Http;

    using Microsoft.AspNet.WebApi.MessageHandlers.Compression;
    using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Compressors;

    using Newtonsoft.Json;

    using NUnit.Framework;

    [TestFixture]
    public class CompressionHandlerTests1
    {
        private HttpServer server;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            var config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            config.MessageHandlers.Insert(0, new ServerCompressionHandler(new GZipCompressor(), new DeflateCompressor()));

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
        public async void Get_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent()
        {
            var client = new HttpClient(new ClientCompressionHandler(this.server, new GZipCompressor(), new DeflateCompressor()));

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            var response = await client.GetAsync("http://localhost:55399/api/test");

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Content-Length: {0}", response.Content.Headers.ContentLength);

            var result = JsonConvert.DeserializeObject<TestModel>(content);

            Assert.AreEqual("Get()", result.Data);
        }
        
        [TestCase("1")]
        [TestCase("10")]
        public async void GetSpecific_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string id)
        {
            var client = new HttpClient(new ClientCompressionHandler(this.server, new GZipCompressor(), new DeflateCompressor()));

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            var response = await client.GetAsync("http://localhost:55399/api/test/" + id);

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Content-Length: {0}", response.Content.Headers.ContentLength);

            var result = JsonConvert.DeserializeObject<TestModel>(content);

            Assert.AreEqual("Get(" + id + ")", result.Data);
        }

        [TestCase("Content1")]
        [TestCase("Content10")]
        [TestCase("Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10")]
        public async void Post_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string body)
        {
            var client = new HttpClient(new ClientCompressionHandler(this.server, new GZipCompressor(), new DeflateCompressor()));

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            var response = await client.PostAsync("http://localhost:55399/api/test", new StringContent(JsonConvert.SerializeObject(new TestModel(body)), Encoding.UTF8, "application/json"));

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Content-Length: {0}", response.Content.Headers.ContentLength);

            var result = JsonConvert.DeserializeObject<TestModel>(content);

            Assert.AreEqual(body, result.Data);
        }

        [TestCase("Content1")]
        [TestCase("Content10")]
        [TestCase("Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10")]
        public async void Post_WhenNestedMessageHandlerIsConfigured_ShouldReturnCompressedContent(string body)
        {
            var client = new HttpClient(new TraceMessageHandler(new ClientCompressionHandler(this.server, new GZipCompressor(), new DeflateCompressor())));

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            var response = await client.PostAsync("http://localhost:55399/api/test", new StringContent(JsonConvert.SerializeObject(new TestModel(body)), Encoding.UTF8, "application/json"));

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Content-Length: {0}", response.Content.Headers.ContentLength);

            var result = JsonConvert.DeserializeObject<TestModel>(content);

            Assert.AreEqual(body, result.Data);
        }

        [TestCase("1", "Content1")]
        [TestCase("2", "Content10")]
        public async void Put_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string id, string body)
        {
            var client = new HttpClient(new ClientCompressionHandler(this.server, new GZipCompressor(), new DeflateCompressor()));

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            var response = await client.PutAsync("http://localhost:55399/api/test/" + id, new StringContent(JsonConvert.SerializeObject(new TestModel(body)), Encoding.UTF8, "application/json"));

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Content-Length: {0}", response.Content.Headers.ContentLength);

            var result = JsonConvert.DeserializeObject<TestModel>(content);

            Assert.AreEqual(body, result.Data);
        }

        [TestCase("1")]
        [TestCase("2")]
        public async void Delete_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string id)
        {
            var client = new HttpClient(new ClientCompressionHandler(this.server, new GZipCompressor(), new DeflateCompressor()));

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            var response = await client.DeleteAsync("http://localhost:55399/api/test/" + id);

            Assert.IsNull(response.Content);
            Assert.IsTrue(response.IsSuccessStatusCode);
        }
    }
}
