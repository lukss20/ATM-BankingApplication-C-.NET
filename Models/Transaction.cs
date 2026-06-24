namespace BankingApplication.Models;

public class Transaction
{
    public DateTime Date { get; set; } = DateTime.Now;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public string Description { get; set; } = string.Empty;
}
