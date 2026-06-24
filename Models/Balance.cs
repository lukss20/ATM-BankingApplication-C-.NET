namespace BankingApplication.Models;

public class Balance
{
    public decimal GEL { get; set; }
    public decimal USD { get; set; }
    public decimal EUR { get; set; }

    public decimal GetAmount(Currency currency)
    {
        return currency switch
        {
            Currency.GEL => GEL,
            Currency.USD => USD,
            Currency.EUR => EUR,
            _ => 0
        };
    }

    public void AddAmount(Currency currency, decimal amount)
    {
        switch (currency)
        {
            case Currency.GEL:
                GEL += amount;
                break;
            case Currency.USD:
                USD += amount;
                break;
            case Currency.EUR:
                EUR += amount;
                break;
        }
    }

    public void SubtractAmount(Currency currency, decimal amount)
    {
        AddAmount(currency, -amount);
    }
}
