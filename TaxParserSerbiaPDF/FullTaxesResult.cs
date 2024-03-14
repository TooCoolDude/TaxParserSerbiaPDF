using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxParserSerbiaPDF
{
    internal class FullTaxesResult : IFullTaxesResult
    {
        public ISocialTaxesResult SocialTaxesResult { get; }

        public IIncomeTaxResult IncomeTaxResult { get; }

        public FullTaxesResult(IIncomeTaxResult incomeTaxResult, ISocialTaxesResult socialTaxesResult)
        {
            IncomeTaxResult = incomeTaxResult;
            SocialTaxesResult = socialTaxesResult;
        }
    }
}
