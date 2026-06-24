using System.Globalization;
using BankingApplication.Models;

namespace BankingApplication.Services;

public class ValidationService
{
    public bool IsCardNumberValid(string cardNumber)
    {
        return !string.IsNullOrWhiteSpace(cardNumber)
               && cardNumber.Length == 16
               && cardNumber.All(char.IsDigit);
    }

    public bool IsExpirationDateValid(string expirationDate)
    {
        if (string.IsNullOrWhiteSpace(expirationDate))
        {
            return false;
        }

        string[] formats = { "MM/yy", "MM/yyyy" };
        if (!DateTime.TryParseExact(expirationDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
        {
            return false;
        }

        DateTime lastValidDay = new(parsedDate.Year, parsedDate.Month, DateTime.DaysInMonth(parsedDate.Year, parsedDate.Month), 23, 59, 59);
        return lastValidDay >= DateTime.Now;
    }

    public bool IsCvcValid(string cvc)
    {
        return !string.IsNullOrWhiteSpace(cvc)
               && cvc.Length == 3
               && cvc.All(char.IsDigit);
    }

    public bool IsPinValid(string pin)
    {
        return !string.IsNullOrWhiteSpace(pin)
               && pin.Length == 4
               && pin.All(char.IsDigit);
    }

    public bool TryReadCurrency(string? input, out Currency currency)
    {
        currency = Currency.GEL;

        if (int.TryParse(input, out int number) && Enum.IsDefined(typeof(Currency), number))
        {
            currency = (Currency)number;
            return true;
        }

        return false;
    }
}
