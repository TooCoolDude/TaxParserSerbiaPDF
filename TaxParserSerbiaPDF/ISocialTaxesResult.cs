public interface ISocialTaxesResult
{
    int FirstYear {get;}
    DateTime FirstMonthStartDate {get;}
    DateTime FirstMonthEndDate {get;}

    decimal FirstMonthPensionPayment {get;}
    decimal RegularPensionPayment {get;}
    byte[] FirstYearPensionQRpng {get;}
    byte[] NextYearPensionQRpng {get;}

    decimal FirstMonthHealthPayment {get;}
    decimal RegularHealthPayment {get;}
    byte[] FirstYearHealthQRpng {get;}
    byte[] NextYearHealthQRpng {get;}

    decimal FirstMonthUnemploymentPayment {get;}
    decimal RegularUnemploymentPayment {get;}
    byte[] FirstYearUnemploymentQRpng {get;}
    byte[] NextYearUnemploymentQRpng {get;}
}