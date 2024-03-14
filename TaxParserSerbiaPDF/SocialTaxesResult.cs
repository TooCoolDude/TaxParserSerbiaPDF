using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxParserSerbiaPDF
{
    internal class SocialTaxesResult : ISocialTaxesResult
    {
        public int FirstYear { get; internal set; }

        public DateTime FirstMonthStartDate { get; internal set; }

        public DateTime FirstMonthEndDate { get; internal set; }


        public decimal FirstMonthPensionPayment { get; internal set; }

        public decimal RegularPensionPayment { get; internal set; }

        public byte[] FirstYearPensionQRpng { get; internal set; }

        public byte[] NextYearPensionQRpng { get; internal set; }


        public decimal FirstMonthHealthPayment { get; internal set; }

        public decimal RegularHealthPayment { get; internal set; }

        public byte[] FirstYearHealthQRpng { get; internal set; }

        public byte[] NextYearHealthQRpng { get; internal set; }


        public decimal FirstMonthUnemploymentPayment { get; internal set; }

        public decimal RegularUnemploymentPayment { get; internal set; }

        public byte[] FirstYearUnemploymentQRpng { get; internal set; }

        public byte[] NextYearUnemploymentQRpng { get; internal set; }
    }
}
