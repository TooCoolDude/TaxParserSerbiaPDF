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

                var taxesValuesPage = pages.First();
                var QRsPage = pages.Last();

                var taxesValuesPageText = FixStringChars(GetPageText(taxesValuesPage));
                var QRsPageText = FixStringChars(GetPageText(QRsPage));

                var images = QRsPage.GetImages().ToArray();
                var base64Images = GetQRsPNG(images);

                var firstYear = Regex.Matches(QRsPageText, @$"(?<={FixStringChars("Порез на паушални приход за")})\s+(.*?)\s+(?=97)")[0].Value.Substring(1, 4);
                
                var firstMonthStartDate = Regex.Match(taxesValuesPageText, @$"(?<={FixStringChars("приход за период од")})\s+(.*?)\s+(?={FixStringChars("до")})").Value.Substring(1, 10);
                
                var firstMonthEndDate = Regex.Match(taxesValuesPageText, @$"(?<={FixStringChars("приход за период од")})\s+(.*?)\s+{FixStringChars("године")}(?=)").Value.Substring(16, 10);
                
                var firstMonthPayment = Regex.Match(taxesValuesPageText, @$"(?<={Regex.Escape(FixStringChars("(1. x 10%)"))})(.*?)(?=\n)").Value.Trim().Replace(".", "").Replace(",", ".");
                
                var regularPaymentMatch = Regex.Match(taxesValuesPageText, @$"(?<={FixStringChars("пореза на доходак грађана")})\s+(.*?)\s+(?={FixStringChars("Aконтациja")})");
                var regularPayment = regularPaymentMatch.Success ? 
                    Regex.Match(taxesValuesPageText, @$"(?<={FixStringChars("пореза на доходак грађана")})\s+(.*?)\s+(?={FixStringChars("Aконтациja")})").Value.Split(" ")[4].Replace(".", "").Replace(",", ".")
                    : firstMonthPayment;

                taxResult.FirstYear = int.Parse(firstYear);
                taxResult.FirstYearQRpng = base64Images[0];
                taxResult.NextYearQRpng = base64Images[1];
                taxResult.FirstMonthStartDate = DateTime.ParseExact(firstMonthStartDate,"dd.mm.yyyy", CultureInfo.InvariantCulture);
                taxResult.FirstMonthEndDate = DateTime.ParseExact(firstMonthEndDate, "dd.mm.yyyy", CultureInfo.InvariantCulture);
                taxResult.FirstMonthPayment = decimal.Parse(firstMonthPayment, NumberStyles.Any, CultureInfo.InvariantCulture);
                taxResult.RegularPayment = decimal.Parse(regularPayment, NumberStyles.Any, CultureInfo.InvariantCulture);
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
                var QRsNextYear = GetQRsPNG(images2);

                var firstYear = Regex.Matches(QRsPageText1, @"(?<=ПРИМЕРИ ПОПУЊЕНИХ УПЛАТНИЦА ЗА ПЛАЋАЊЕ ДОПРИНОСА ЗА)\s+(.*?)\s+(?=ГОДИНУ)")[0].Value.Substring(1, 4);
                var firstMonthStartDate = Regex.Match(taxesValuesPageText, @"(?<=I УТВРЂУЈЕ СЕ аконтационо задужење доприноса за обавезно социјално осигурање за \nпериод од)\s+(.*?)\s+(?=до)").Value.Substring(1, 10);
                var firstMonthEndDate = Regex.Match(taxesValuesPageText, @"(?<=I УТВРЂУЈЕ СЕ аконтационо задужење доприноса за обавезно социјално осигурање за \nпериод од)\s+(.*?)\s+године(?=)").Value.Substring(16, 10);
               
                var firstMonthPayments = Regex.Match(taxesValuesPageText, @"(?<=Јануар)\s+(.*?)\s+\n(?=)").Value.Split(" ").Select(x => x.Replace(".", "").Replace(",", ".")).ToArray();
                var regularPayments = Regex.Match(taxesValuesPageText, @"(?<=Фебруар)\s+(.*?)\s+\n(?=)").Value.Split(" ").Select(x => x.Replace(".", "").Replace(",", ".")).ToArray();

                taxesResult.FirstYear = int.Parse(firstYear);
                taxesResult.FirstMonthStartDate = DateTime.ParseExact(firstMonthStartDate, "dd.mm.yyyy", CultureInfo.InvariantCulture);
                taxesResult.FirstMonthEndDate = DateTime.ParseExact(firstMonthEndDate, "dd.mm.yyyy", CultureInfo.InvariantCulture);
                
                taxesResult.FirstMonthPensionPayment = decimal.Parse(firstMonthPayments[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                taxesResult.FirstMonthHealthPayment = decimal.Parse(firstMonthPayments[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                taxesResult.FirstMonthUnemploymentPayment = decimal.Parse(firstMonthPayments[4], NumberStyles.Any, CultureInfo.InvariantCulture);

                taxesResult.RegularPensionPayment = decimal.Parse(regularPayments[2], NumberStyles.Any, CultureInfo.InvariantCulture);
                taxesResult.RegularHealthPayment = decimal.Parse(regularPayments[3], NumberStyles.Any, CultureInfo.InvariantCulture);
                taxesResult.RegularUnemploymentPayment = decimal.Parse(regularPayments[4], NumberStyles.Any, CultureInfo.InvariantCulture);

                taxesResult.FirstYearPensionQRpng = QRsFirstYear[0];
                taxesResult.FirstYearHealthQRpng = QRsFirstYear[1];
                taxesResult.FirstYearUnemploymentQRpng = QRsFirstYear[2];

                taxesResult.NextYearPensionQRpng = QRsNextYear[0];
                taxesResult.NextYearHealthQRpng = QRsNextYear[1];
                taxesResult.NextYearUnemploymentQRpng = QRsNextYear[2];
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

    private static string FixStringChars(string input)
    {
        StringBuilder builder = new StringBuilder(input);

        var pairs = EnToRuChars;
        foreach (char c in pairs.Keys) 
        {
            builder.Replace(c, pairs[c]);
        }

        return builder.ToString();
    }

    private static Dictionary<char, char> EnToRuChars => new Dictionary<char, char> 
    {
        ['a'] = 'а',
        ['A'] = 'А'
    };
}
