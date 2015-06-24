namespace Tests.Tests.Common
{
    using System;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using global::Tests.Models;

    using Newtonsoft.Json;

    using NUnit.Framework;

    public class TestFixture
    {
        private readonly HttpClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestFixture"/> class.
        /// </summary>
        /// <param name="client">The client.</param>
        public TestFixture(HttpClient client)
        {
            this.client = client;
        }

        public async Task Get_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent()
        {
            var response = await this.client.GetAsync("http://localhost:55399/api/test");

            Console.WriteLine("Content: {0}", await response.Content.ReadAsStringAsync());

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Content-Length: {0}", response.Content.Headers.ContentLength);

            var result = JsonConvert.DeserializeObject<TestModel>(content);

            Assert.AreEqual("Get()", result.Data);
        }

        public async Task GetImage_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent()
        {

            var response = await this.client.GetAsync("http://localhost:55399/api/file/image");

            Console.WriteLine("Content: {0}", await response.Content.ReadAsStringAsync());

            Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "image/png");
            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsByteArrayAsync();

            Console.WriteLine("Content-Length: {0}", response.Content.Headers.ContentLength);

            Assert.AreEqual(749, response.Content.Headers.ContentLength);
            Assert.AreEqual(4596, content.Length);
        }

        public async Task GetImage_WhenAttributeIsConfigured_ShouldReturnUncompressedContent()
        {

            var response = await this.client.GetAsync("http://localhost:55399/api/file/uncompressedimage");

            Console.WriteLine("Content: {0}", await response.Content.ReadAsStringAsync());

            Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "image/png");
            Assert.IsFalse(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsByteArrayAsync();

            Console.WriteLine("Content-Length: {0}", response.Content.Headers.ContentLength);

            Assert.AreEqual(4596, content.Length);
        }

        public async Task GetPdf_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent()
        {
            var response = await this.client.GetAsync("http://localhost:55399/api/file/pdf");

            Console.WriteLine("Content: {0}", await response.Content.ReadAsStringAsync());

            Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "application/pdf");
            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsByteArrayAsync();

            Console.WriteLine("Content-Length: {0}", response.Content.Headers.ContentLength);

            Assert.AreEqual(16538, content.Length);
        }

        public async Task GetSpecific_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string id)
        {
            var response = await this.client.GetAsync("http://localhost:55399/api/test/" + id);

            Console.WriteLine("Content: {0}", await response.Content.ReadAsStringAsync());

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Content-Length: {0}", response.Content.Headers.ContentLength);

            var result = JsonConvert.DeserializeObject<TestModel>(content);

            Assert.AreEqual("Get(" + id + ")", result.Data);
        }

        public async Task Post_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string body)
        {
            var response = await this.client.PostAsync("http://localhost:55399/api/test", new StringContent(JsonConvert.SerializeObject(new TestModel(body)), Encoding.UTF8, "application/json"));

            Console.WriteLine("Content: {0}", await response.Content.ReadAsStringAsync());

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Content-Length: {0}", response.Content.Headers.ContentLength);

            var result = JsonConvert.DeserializeObject<TestModel>(content);

            Assert.AreEqual(body, result.Data);
        }

        public async Task Post_WhenNestedMessageHandlerIsConfigured_ShouldReturnCompressedContent(string body)
        {
            var response = await this.client.PostAsync("http://localhost:55399/api/test", new StringContent(JsonConvert.SerializeObject(new TestModel(body)), Encoding.UTF8, "application/json"));

            Console.WriteLine("Content: {0}", await response.Content.ReadAsStringAsync());

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Content-Length: {0}", response.Content.Headers.ContentLength);

            var result = JsonConvert.DeserializeObject<TestModel>(content);

            Assert.AreEqual(body, result.Data);
        }

        public async Task Put_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string id, string body)
        {
            var response = await this.client.PutAsync("http://localhost:55399/api/test/" + id, new StringContent(JsonConvert.SerializeObject(new TestModel(body)), Encoding.UTF8, "application/json"));

            Console.WriteLine("Content: {0}", await response.Content.ReadAsStringAsync());

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Content-Length: {0}", response.Content.Headers.ContentLength);

            var result = JsonConvert.DeserializeObject<TestModel>(content);

            Assert.AreEqual(body, result.Data);
        }

        public async Task Delete_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string id)
        {
            var response = await this.client.DeleteAsync("http://localhost:55399/api/test/" + id);

            Assert.That(response.Content == null || string.IsNullOrEmpty(await response.Content.ReadAsStringAsync()));
            Assert.IsTrue(response.IsSuccessStatusCode);
        }
    }
}
