public interface IIncomeTaxResult
{
    int FirstYear {get;}
    DateTime FirstMonthStartDate {get;}
    DateTime FirstMonthEndDate {get;}
    decimal FirstMonthPayment {get;}
    decimal RegularPayment {get;}
    byte[] FirstYearQRpng {get;}
    byte[] NextYearQRpng {get;}
}