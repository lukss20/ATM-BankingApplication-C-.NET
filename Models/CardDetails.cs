namespace BankingApplication.Models;

public class CardDetails
{
    public string CardNumber { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty;
    public string CVC { get; set; } = string.Empty;
}
