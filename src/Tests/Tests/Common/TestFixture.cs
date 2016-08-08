namespace Tests.Tests.Common
{
    using global::Tests.Extensions;
    using global::Tests.Models;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using System;
    using System.Net.Http;
    using System.Text;

    public abstract class TestFixture
    {
        public HttpClient Client { get; set; }

        [Test]
        public async void Get_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent()
        {
            var response = await this.Client.GetAsync("http://localhost:55399/api/test");

            Console.Write(await response.ToTestString());

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var result = JsonConvert.DeserializeObject<TestModel>(await response.Content.ReadAsStringAsync());

            Assert.AreEqual("Get()", result.Data);
        }

        [Test]
        public async void Get_WhenGivenCustomHeader_ShouldReturnCompressedContentWithCustomHeader()
        {
            var response = await this.Client.GetAsync("http://localhost:55399/api/test/customheader");

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            Console.Write(await response.ToTestString());

            Assert.IsTrue(response.Headers.Contains("DataServiceVersion"), "The response did not contain the DataServiceVersion header");
        }

        [Test]
        public async void Get_WhenGivenAcceptEncodingNull_ShouldReturnUncompressedContent()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:55399/api/test");
            this.Client.DefaultRequestHeaders.AcceptEncoding.Clear();

            Console.WriteLine("Accept-Encoding: {0}", string.Join(", ", request.Headers.AcceptEncoding));

            var response = await this.Client.SendAsync(request);

            Console.Write(await response.ToTestString());

            Assert.IsFalse(response.Content.Headers.ContentEncoding.Contains("gzip"), "The server returned compressed content");
        }

        [Test]
        public async void Get_WhenResponseHeaderIsModified_ShouldReturnModifiedResponse()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:55399/api/test/redirect");

            var response = await this.Client.SendAsync(request);

            Console.Write(await response.ToTestString());

            Assert.AreEqual(29, response.Content.Headers.ContentLength);
            Assert.AreEqual("http://localhost:55399/api/test", response.Headers.Location.ToString());
        }

        [Test]
        public async void GetImage_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent()
        {
            var response = await this.Client.GetAsync("http://localhost:55399/api/file/image");

            Console.Write(await response.ToTestString());

            Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "image/png");
            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsByteArrayAsync();

            Assert.AreEqual(749, response.Content.Headers.ContentLength);
            Assert.AreEqual(4596, content.Length);
        }

        [Test]
        public async void GetImage_WhenAttributeIsConfigured_ShouldReturnUncompressedContent()
        {

            var response = await this.Client.GetAsync("http://localhost:55399/api/file/uncompressedimage");

            Console.Write(await response.ToTestString());

            Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "image/png");
            Assert.IsFalse(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsByteArrayAsync();

            Assert.AreEqual(4596, content.Length);
        }

        [Test]
        public async void GetPdf_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent()
        {
            var response = await this.Client.GetAsync("http://localhost:55399/api/file/pdf");

            Console.Write(await response.ToTestString());

            Assert.IsTrue(response.Content.Headers.ContentType.MediaType == "application/pdf");
            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsByteArrayAsync();

            Assert.AreEqual(16538, content.Length);
        }

        [TestCase("1")]
        [TestCase("10")]
        public async void GetSpecific_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string id)
        {
            var response = await this.Client.GetAsync("http://localhost:55399/api/test/" + id);

            Console.Write(await response.ToTestString());

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<TestModel>(content);

            Assert.AreEqual("Get(" + id + ")", result.Data);
        }

        [TestCase("Content1")]
        [TestCase("Content10")]
        [TestCase("Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10")]
        public async void Post_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string body)
        {
            var response = await this.Client.PostAsync("http://localhost:55399/api/test", new StringContent(JsonConvert.SerializeObject(new TestModel(body)), Encoding.UTF8, "application/json"));

            Console.Write(await response.ToTestString());

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<TestModel>(content);

            Assert.AreEqual(body, result.Data);
        }

        [TestCase("Content1")]
        [TestCase("Content10")]
        [TestCase("Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10Content10")]
        public async void Post_WhenNestedMessageHandlerIsConfigured_ShouldReturnCompressedContent(string body)
        {
            var response = await this.Client.PostAsync("http://localhost:55399/api/test", new StringContent(JsonConvert.SerializeObject(new TestModel(body)), Encoding.UTF8, "application/json"));

            Console.Write(await response.ToTestString());

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<TestModel>(content);

            Assert.AreEqual(body, result.Data);
        }

        [TestCase("1", "Content1")]
        [TestCase("2", "Content10")]
        public async void Put_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string id, string body)
        {
            var response = await this.Client.PutAsync("http://localhost:55399/api/test/" + id, new StringContent(JsonConvert.SerializeObject(new TestModel(body)), Encoding.UTF8, "application/json"));

            Console.Write(await response.ToTestString());

            Assert.IsTrue(response.Content.Headers.ContentEncoding.Contains("gzip"));

            var content = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<TestModel>(content);

            Assert.AreEqual(body, result.Data);
        }

        [TestCase("1")]
        [TestCase("2")]
        public async void Delete_WhenMessageHandlerIsConfigured_ShouldReturnCompressedContent(string id)
        {
            var response = await this.Client.DeleteAsync("http://localhost:55399/api/test/" + id);

            Assert.That(response.Content == null || string.IsNullOrEmpty(await response.Content.ReadAsStringAsync()));
            Assert.IsTrue(response.IsSuccessStatusCode);
        }
    }
}
