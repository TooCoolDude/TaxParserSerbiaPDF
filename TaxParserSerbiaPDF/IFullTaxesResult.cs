using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxParserSerbiaPDF;
public interface IFullTaxesResult
{
    ISocialTaxesResult SocialTaxesResult { get; }

    IIncomeTaxResult IncomeTaxResult { get; }
}
