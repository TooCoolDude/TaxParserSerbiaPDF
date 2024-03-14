using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using TaxParserSerbiaPDF;
using System.Globalization;

namespace TaxParserSerbiaPDF;
public static class PdfParser
{
    public static IFullTaxesResult GetAllTaxes(Stream incomeTaxPDFStream, Stream socialTaxesPDFStream)
    {
        return new FullTaxesResult(GetIncomeTaxResult(incomeTaxPDFStream), GetSocialTaxesResult(socialTaxesPDFStream));
    }

    public static IIncomeTaxResult GetIncomeTaxResult(Stream incomeTaxPDFStream)
    {
        var taxResult = new IncomeTaxResult();

        try
        {
            using (PdfDocument doc = PdfDocument.Open(incomeTaxPDFStream))
            {
                var pages = doc.GetPages().ToArray();

                var taxesValuesPage = pages[0];
                var QRsPage = pages[3];

                var taxesValuesPageText = GetPageText(taxesValuesPage);
                var QRsPageText = GetPageText(QRsPage);

                var images = QRsPage.GetImages().ToArray();
                var base64Images = GetQRsPNG(images);

                var firstYear = Regex.Matches(QRsPageText, @"(?<=Порез на паушални приход зa)\s+(.*?)\s+(?=97)")[0].Value.Substring(1, 4);
                var firstMonthStartDate = Regex.Match(taxesValuesPageText, @"(?<=Обрачуната аконтација пореза за период \nод)\s+(.*?)\s+(?=до)").Value.Substring(1, 10);
                var firstMonthEndDate = Regex.Match(taxesValuesPageText, @"(?<=Обрачуната аконтација пореза за период \nод)\s+(.*?)\s+\((?=)").Value.Substring(16, 10);
                var firstMonthPayment = Regex.Match(taxesValuesPageText, @"(?<=Обрачуната аконтација пореза за период \nод)\s+(.*?)\s+(?=3\.)").Value.Split(" ")[7].Replace(".", "").Replace(",", ".");
                var regularPayment = Regex.Match(taxesValuesPageText, @"(?<=пореза на доходак грађана)\s+(.*?)\s+(?=Aконтациja)").Value.Split(" ")[4].Replace(".", "").Replace(",", ".");

                taxResult.FirstYear = int.Parse(firstYear);
                taxResult.FirstYearQRpng = base64Images[0];
                taxResult.NextYearQRpng = base64Images[1];
                taxResult.FirstMonthStartDate = DateTime.ParseExact(firstMonthStartDate,"dd.mm.yyyy", CultureInfo.InvariantCulture);
                taxResult.FirstMonthEndDate = DateTime.ParseExact(firstMonthEndDate, "dd.mm.yyyy", CultureInfo.InvariantCulture);
                taxResult.FirstMonthPayment = decimal.Parse(firstMonthPayment);
                taxResult.RegularPayment = decimal.Parse(regularPayment);
            }
        }

        catch (Exception)
        {
            throw;
        }

        return taxResult;
    }

    public static ISocialTaxesResult GetSocialTaxesResult(Stream socialTaxesPDFStream)
    {
        var taxesResult = new SocialTaxesResult();

        try
        {
            using (PdfDocument doc = PdfDocument.Open(socialTaxesPDFStream))
            {
                var pages = doc.GetPages().ToArray();

                var taxesValuesPage = pages[0];
                var QRsPage1 = pages[4];
                var QRsPage2 = pages[5];

                var taxesValuesPageText = GetPageText(taxesValuesPage);
                var QRsPageText1 = GetPageText(QRsPage1);
                var QRsPageText2 = GetPageText(QRsPage2);

                var images1 = QRsPage1.GetImages().ToArray();
                var QRsFirstYear = GetQRsPNG(images1);

                var images2 = QRsPage2.GetImages().ToArray();
                var QEsNextYear = GetQRsPNG(images2);

                var firstYear = Regex.Matches(QRsPageText1, @"(?<=ПРИМЕРИ ПОПУЊЕНИХ УПЛАТНИЦА ЗА ПЛАЋАЊЕ ДОПРИНОСА ЗА)\s+(.*?)\s+(?=ГОДИНУ)")[0].Value.Substring(1, 4);
                var firstMonthStartDate = Regex.Match(taxesValuesPageText, @"(?<=I УТВРЂУЈЕ СЕ аконтационо задужење доприноса за обавезно социјално осигурање за \nпериод од)\s+(.*?)\s+(?=до)").Value.Substring(1, 10);
                var firstMonthEndDate = Regex.Match(taxesValuesPageText, @"(?<=I УТВРЂУЈЕ СЕ аконтационо задужење доприноса за обавезно социјално осигурање за \nпериод од)\s+(.*?)\s+године(?=)").Value.Substring(16, 10);
               
                var firstMonthPayments = Regex.Match(taxesValuesPageText, @"(?<=Јануар)\s+(.*?)\s+\n(?=)").Value.Split(" ").Select(x => x.Replace(".", "").Replace(",", ".")).ToArray();
                var regularPayments = Regex.Match(taxesValuesPageText, @"(?<=Фебруар)\s+(.*?)\s+\n(?=)").Value.Split(" ").Select(x => x.Replace(".", "").Replace(",", ".")).ToArray();

                taxesResult.FirstYear = int.Parse(firstYear);
                taxesResult.FirstMonthStartDate = DateTime.ParseExact(firstMonthStartDate, "dd.mm.yyyy", CultureInfo.InvariantCulture);
                taxesResult.FirstMonthEndDate = DateTime.ParseExact(firstMonthEndDate, "dd.mm.yyyy", CultureInfo.InvariantCulture);
                
                taxesResult.FirstMonthPensionPayment = decimal.Parse(firstMonthPayments[2]);
                taxesResult.FirstMonthHealthPayment = decimal.Parse(firstMonthPayments[3]);
                taxesResult.FirstMonthUnemploymentPayment = decimal.Parse(firstMonthPayments[4]);

                taxesResult.RegularPensionPayment = decimal.Parse(regularPayments[2]);
                taxesResult.RegularHealthPayment = decimal.Parse(regularPayments[3]);
                taxesResult.RegularUnemploymentPayment = decimal.Parse(regularPayments[4]);

                taxesResult.FirstYearPensionQRpng = QRsFirstYear[0];
                taxesResult.FirstYearHealthQRpng = QRsFirstYear[1];
                taxesResult.FirstYearUnemploymentQRpng = QRsFirstYear[2];

                taxesResult.NextYearPensionQRpng = QEsNextYear[0];
                taxesResult.NextYearHealthQRpng = QEsNextYear[1];
                taxesResult.NextYearUnemploymentQRpng = QEsNextYear[2];
            }
        }

        catch (Exception)
        {
            throw;
        }

        return taxesResult;
    }

    private static byte[][] GetQRsPNG(IPdfImage[] images)
    {
        List<byte[]> QRsPNG = new();
        foreach (var image in images)
        {
            if (!image.TryGetPng(out var QR))
                continue;

            QRsPNG.Add(QR);
        }
        return QRsPNG.ToArray();
    }

    private static string GetPageText(Page page)
    {
        StringBuilder builder = new StringBuilder();

        var wordsList = page.GetWords().GroupBy(x => x.BoundingBox.Bottom);

        foreach (var word in wordsList)
        {
            foreach (var item in word)
            {
                builder.Append(item.Text + " ");
            }
            builder.Append("\n");
        }

        string text = builder.ToString();
            
        return text;
    }
}
