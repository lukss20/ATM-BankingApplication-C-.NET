using BankingApplication.Models;

namespace BankingApplication.Services;

public class BankService
{
    private readonly List<User> _users;
    private readonly JsonStorageService _storageService;
    private readonly LoggerService _logger;

    private const decimal UsdToGel = 2.70m;
    private const decimal EurToGel = 2.95m;

    public BankService(List<User> users, JsonStorageService storageService, LoggerService logger)
    {
        _users = users;
        _storageService = storageService;
        _logger = logger;
    }

    public User? FindUserByCard(string cardNumber, string expirationDate, string cvc)
    {
        return _users.FirstOrDefault(user =>
            user.CardDetails.CardNumber == cardNumber &&
            user.CardDetails.ExpirationDate == expirationDate &&
            user.CardDetails.CVC == cvc);
    }

    public bool IsPinCorrect(User user, string pin)
    {
        return user.PinCode == pin;
    }

    public void ShowBalances(User user)
    {
        Console.WriteLine();
        Console.WriteLine("Your balances:");
        Console.WriteLine($"GEL: {user.Balances.GEL:F2}");
        Console.WriteLine($"USD: {user.Balances.USD:F2}");
        Console.WriteLine($"EUR: {user.Balances.EUR:F2}");
    }

    public bool Deposit(User user, Currency currency, decimal amount)
    {
        if (amount <= 0)
        {
            Console.WriteLine("Amount must be greater than zero.");
            return false;
        }

        user.Balances.AddAmount(currency, amount);
        AddTransaction(user, TransactionType.Deposit, amount, currency, $"Deposited {amount:F2} {currency}");
        SaveChanges("Deposit completed");
        _logger.Log($"Deposit: {user.FullName}, {amount:F2} {currency}");
        return true;
    }

    public bool Withdraw(User user, Currency currency, decimal amount)
    {
        if (amount <= 0)
        {
            Console.WriteLine("Amount must be greater than zero.");
            return false;
        }

        if (amount > user.Balances.GetAmount(currency))
        {
            Console.WriteLine("Insufficient balance.");
            return false;
        }

        user.Balances.SubtractAmount(currency, amount);
        AddTransaction(user, TransactionType.Withdrawal, amount, currency, $"Withdrew {amount:F2} {currency}");
        SaveChanges("Withdrawal completed");
        _logger.Log($"Withdrawal: {user.FullName}, {amount:F2} {currency}");
        return true;
    }

    public bool ChangePin(User user, string oldPin, string newPin, ValidationService validationService)
    {
        if (user.PinCode != oldPin)
        {
            Console.WriteLine("Old PIN is incorrect.");
            _logger.Log($"Failed PIN change for {user.FullName}");
            return false;
        }

        if (!validationService.IsPinValid(newPin))
        {
            Console.WriteLine("New PIN must be exactly 4 digits.");
            return false;
        }

        user.PinCode = newPin;
        AddTransaction(user, TransactionType.ChangePin, 0, Currency.GEL, "PIN code was changed");
        SaveChanges("PIN changed");
        _logger.Log($"PIN change: {user.FullName}");
        return true;
    }

    public bool ConvertCurrency(User user, Currency sourceCurrency, Currency targetCurrency, decimal amount)
    {
        if (sourceCurrency == targetCurrency)
        {
            Console.WriteLine("Source and target currencies must be different.");
            return false;
        }

        if (amount <= 0)
        {
            Console.WriteLine("Amount must be greater than zero.");
            return false;
        }

        if (amount > user.Balances.GetAmount(sourceCurrency))
        {
            Console.WriteLine("Insufficient balance for conversion.");
            return false;
        }

        decimal convertedAmount = ConvertAmount(amount, sourceCurrency, targetCurrency);

        user.Balances.SubtractAmount(sourceCurrency, amount);
        user.Balances.AddAmount(targetCurrency, convertedAmount);
        AddTransaction(user, TransactionType.CurrencyConversion, amount, sourceCurrency,
            $"Converted {amount:F2} {sourceCurrency} to {convertedAmount:F2} {targetCurrency}");
        SaveChanges("Currency conversion completed");

        Console.WriteLine($"Converted {amount:F2} {sourceCurrency} to {convertedAmount:F2} {targetCurrency}.");
        _logger.Log($"Currency conversion: {user.FullName}, {amount:F2} {sourceCurrency} to {convertedAmount:F2} {targetCurrency}");
        return true;
    }

    public void ShowLastFiveTransactions(User user)
    {
        Console.WriteLine();
        Console.WriteLine("Last 5 transactions:");

        List<Transaction> latestTransactions = user.TransactionHistory
            .OrderByDescending(transaction => transaction.Date)
            .Take(5)
            .ToList();

        if (latestTransactions.Count == 0)
        {
            Console.WriteLine("No transactions yet.");
            return;
        }

        foreach (Transaction transaction in latestTransactions)
        {
            Console.WriteLine($"{transaction.Date:yyyy-MM-dd HH:mm} | {transaction.Type} | {transaction.Amount:F2} {transaction.Currency} | {transaction.Description}");
        }
    }

    private decimal ConvertAmount(decimal amount, Currency sourceCurrency, Currency targetCurrency)
    {
        decimal amountInGel = sourceCurrency switch
        {
            Currency.GEL => amount,
            Currency.USD => amount * UsdToGel,
            Currency.EUR => amount * EurToGel,
            _ => amount
        };

        decimal convertedAmount = targetCurrency switch
        {
            Currency.GEL => amountInGel,
            Currency.USD => amountInGel / UsdToGel,
            Currency.EUR => amountInGel / EurToGel,
            _ => amountInGel
        };

        return Math.Round(convertedAmount, 2);
    }

    private void AddTransaction(User user, TransactionType type, decimal amount, Currency currency, string description)
    {
        user.TransactionHistory.Add(new Transaction
        {
            Date = DateTime.Now,
            Type = type,
            Amount = amount,
            Currency = currency,
            Description = description
        });
    }

    private void SaveChanges(string successMessage)
    {
        bool saved = _storageService.SaveUsers(_users);

        if (saved)
        {
            Console.WriteLine(successMessage);
        }
        else
        {
            Console.WriteLine("Operation completed, but the data could not be saved. Please contact support.");
        }
    }
}
