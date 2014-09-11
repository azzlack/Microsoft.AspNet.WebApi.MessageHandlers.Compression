namespace Tests.Models
{
    using iTextSharp.text;
    using iTextSharp.text.pdf;

    public class PdfLayout : PdfPageEventHelper
    {
        public override void OnStartPage(PdfWriter writer, Document doc)
        {
        }

        public override void OnEndPage(PdfWriter writer, Document doc)
        {
            var cb = writer.DirectContent;
            cb.MoveTo(doc.LeftMargin, doc.BottomMargin - 10f);
            cb.LineTo(doc.PageSize.Width - doc.RightMargin, doc.BottomMargin - 10f);
            cb.SetRGBColorStroke(255, 85, 0);
            cb.Stroke();
        }
    }
}