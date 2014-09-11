namespace Tests.Tests.Facade
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Threading;

    using global::Tests.Handlers;
    using global::Tests.Tests.Common;

    using Microsoft.AspNet.WebApi.MessageHandlers.Compression;
    using Microsoft.AspNet.WebApi.MessageHandlers.Compression.Compressors;

    using NUnit.Framework;

    [TestFixture]
    public class WebHostTests
    {
        private Process iisProcess;

        private TestFixture testFixture;

        [TestFixtureSetUp]
        public void Setup()
        {
            var thread = new Thread(this.StartIISExpress) { IsBackground = true };
            thread.Start();

            var client =
                new HttpClient(
                    new TraceMessageHandler(new ClientCompressionHandler(new GZipCompressor(), new DeflateCompressor())))
                    {
                        BaseAddress = new Uri("http://localhost:55399")
                    };

            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

            this.testFixture = new TestFixture(client);
        }

        [TestFixtureTearDown]
        public void Teardown()
        {
            if (!this.iisProcess.HasExited) 
            {
                this.iisProcess.CloseMainWindow();

                if (this.iisProcess != null) 
                {
                    this.iisProcess.Dispose();
                }
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

        /// <summary>
        /// Starts the IIS express.
        /// </summary>
        private void StartIISExpress()
        {
            var assemblyPath = Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path);
            var sitePath = Directory.GetParent(assemblyPath).Parent;

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                LoadUserProfile = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                Arguments = string.Format("/path:\"{0}\" /port:{1}", sitePath.FullName, 55399)
            };

            var programfiles = string.IsNullOrEmpty(startInfo.EnvironmentVariables["programfiles"])
                                ? startInfo.EnvironmentVariables["programfiles(x86)"]
                                : startInfo.EnvironmentVariables["programfiles"];

            startInfo.FileName = programfiles + "\\IIS Express\\iisexpress.exe";

            Console.WriteLine("IIS Express Command: {0} {1}", startInfo.FileName, startInfo.Arguments);

            try
            {
                this.iisProcess = new Process { StartInfo = startInfo };
                this.iisProcess.OutputDataReceived += (sender, args) => Console.WriteLine(args.Data);
                this.iisProcess.ErrorDataReceived += (sender, args) => Console.WriteLine(args.Data);

                this.iisProcess.Start();
                this.iisProcess.WaitForExit();
            }
            catch
            {
                this.iisProcess.CloseMainWindow();
                this.iisProcess.Dispose();
            }
        }
    }
}