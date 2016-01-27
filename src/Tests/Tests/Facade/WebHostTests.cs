namespace Tests.Tests.Facade
{
    using global::Tests.Handlers;
    using global::Tests.Tests.Common;
    using NUnit.Framework;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Extensions.Compression.Client;
    using System.Net.Http.Extensions.Compression.Core.Compressors;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Threading;

    [TestFixture]
    public class WebHostTests : TestFixture
    {
        private Process iisProcess;

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

            this.Client = client;
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