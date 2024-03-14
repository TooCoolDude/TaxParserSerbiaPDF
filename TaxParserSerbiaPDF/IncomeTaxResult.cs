using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UglyToad.PdfPig.AcroForms.Fields;

namespace TaxParserSerbiaPDF
{
    internal class IncomeTaxResult : IIncomeTaxResult
    {
        public int FirstYear { get; internal set; }

        public DateTime FirstMonthStartDate { get; internal set; }

        public DateTime FirstMonthEndDate { get; internal set; }

        public decimal FirstMonthPayment { get; internal set; }

        public decimal RegularPayment { get; internal set; }

        public byte[] FirstYearQRpng { get; internal set; }

        public byte[] NextYearQRpng { get; internal set; }
    }
}
