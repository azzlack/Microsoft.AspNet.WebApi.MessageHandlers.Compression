namespace Tests.Controllers
{
    using global::Tests.Models;
    using iTextSharp.text;
    using iTextSharp.text.pdf;
    using Microsoft.AspNet.WebApi.Extensions.Compression.Server.Attributes;
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Web.Http;

    public class FileController : ApiController
    {
        [Route("api/file/image")]
        public async Task<HttpResponseMessage> GetImage()
        {
            using (var ms = new MemoryStream())
            {
                var baseDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent;
                var filePath = baseDir.FullName + "/Content/Images/app-icon-1024.png";

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fs.Position = 0;
                    await fs.CopyToAsync(ms);
                }

                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(ms.ToArray())
                };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                return result;
            }
        }

        [Compression(Enabled = false)]
        [Route("api/file/uncompressedimage")]
        public async Task<HttpResponseMessage> GetUncompressedImage()
        {
            using (var ms = new MemoryStream())
            {
                var baseDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent;
                var filePath = baseDir.FullName + "/Content/Images/app-icon-1024.png";

                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    fs.Position = 0;
                    await fs.CopyToAsync(ms);
                }

                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(ms.ToArray())
                };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                return result;
            }
        }

        [Route("api/file/pdf")]
        public async Task<HttpResponseMessage> GetPdf()
        {
            var document = new Document(PageSize.A4);
            var pdfstream = new MemoryStream();

            document.SetMargins(document.LeftMargin, document.RightMargin, document.TopMargin, document.BottomMargin + 10f);

            var writer = PdfWriter.GetInstance(document, pdfstream);
            writer.PageEvent = new PdfLayout();

            // Write PDF document
            document.Open();

            var baseDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent;

            var logoHeader = Image.GetInstance(baseDir.FullName + "/Content/Images/app-icon-1024.png");
            logoHeader.ScalePercent(10);
            logoHeader.SetAbsolutePosition(document.PageSize.Width - logoHeader.ScaledWidth - document.RightMargin, document.PageSize.Height - document.TopMargin - logoHeader.ScaledHeight);
            document.Add(logoHeader);

            var headline = new Paragraph("Unicorns!");

            document.Add(headline);

            document.Add(new Paragraph(" "));

            document.Close();

            var result = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(new MemoryStream(pdfstream.ToArray())) };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = "unicorns.pdf"
            };

            return result;
        }
    }
}
