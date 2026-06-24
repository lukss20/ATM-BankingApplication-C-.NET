using System.Text.Json.Serialization;

namespace BankingApplication.Models;

public class User
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public CardDetails CardDetails { get; set; } = new();
    public string PinCode { get; set; } = string.Empty;
    public Balance Balances { get; set; } = new();
    public List<Transaction> TransactionHistory { get; set; } = new();

    [JsonIgnore]
    public string FullName => $"{FirstName} {LastName}".Trim();
}
