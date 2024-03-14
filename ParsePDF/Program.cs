using System.IO;
using System.Net.NetworkInformation;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Graphics;
using UglyToad.PdfPig.XObjects;
using TaxParserSerbiaPDF;

namespace ParsePDF
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string resultFolder = $"{Directory.GetCurrentDirectory()}\\images";

            using (var pdfFile2 = new FileStream("OriginPDF\\6d815dcecd93ab26.pdf", FileMode.Open, FileAccess.Read))
            using (var pdfFile1 = new FileStream("OriginPDF\\9850a56f8247cf65.pdf", FileMode.Open, FileAccess.Read))
            {
                var result = PdfParser.GetAllTaxes(pdfFile1, pdfFile2);
            }

        }
    }
}
