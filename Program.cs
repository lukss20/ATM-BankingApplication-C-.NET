using BankingApplication.Models;
using BankingApplication.Services;

string projectFolder = FindProjectFolder();
string dataFilePath = Path.Combine(projectFolder, "Data", "users.json");
string logFilePath = Path.Combine(projectFolder, "logs.txt");

LoggerService logger = new(logFilePath);
ValidationService validationService = new();
JsonStorageService storageService = new(dataFilePath, logger);
List<User> users = storageService.LoadUsers();
BankService bankService = new(users, storageService, logger);

logger.Log("Application start");

bool applicationIsRunning = true;

while (applicationIsRunning)
{
    try
    {
        SafeClear();
        Console.WriteLine("================================");
        Console.WriteLine("      ATM Banking Application    ");
        Console.WriteLine("================================");
        Console.WriteLine();

        User? loggedInUser = LoginUser(bankService, validationService, logger);

        if (loggedInUser != null)
        {
            ShowMainMenu(loggedInUser, bankService, validationService);
        }

        Console.WriteLine();
        Console.Write("Return to start? (Y/N): ");
        string? answer = Console.ReadLine();

        if (!string.Equals(answer, "Y", StringComparison.OrdinalIgnoreCase))
        {
            applicationIsRunning = false;
        }
    }
    catch (Exception ex)
    {
        logger.Log($"Unexpected error: {ex.Message}");
        Console.WriteLine("Something went wrong, but the application handled it safely.");
        Pause();
    }
}

Console.WriteLine("Thank you for using the ATM Banking Application.");

static User? LoginUser(BankService bankService, ValidationService validationService, LoggerService logger)
{
    Console.Write("Enter card number: ");
    string cardNumber = ReadRequiredText();

    Console.Write("Enter expiration date (MM/YY): ");
    string expirationDate = ReadRequiredText();

    Console.Write("Enter CVC: ");
    string cvc = ReadRequiredText();

    if (!validationService.IsCardNumberValid(cardNumber) ||
        !validationService.IsExpirationDateValid(expirationDate) ||
        !validationService.IsCvcValid(cvc))
    {
        Console.WriteLine("Invalid card number, expiration date, or CVC format.");
        logger.Log($"Failed card validation for card number {cardNumber}");
        return null;
    }

    User? user = bankService.FindUserByCard(cardNumber, expirationDate, cvc);

    if (user == null)
    {
        Console.WriteLine("Card number, expiration date, or CVC was not found.");
        logger.Log($"Failed card validation for card number {cardNumber}");
        return null;
    }

    Console.Write("Enter PIN code: ");
    string pin = ReadRequiredText();

    if (!bankService.IsPinCorrect(user, pin))
    {
        Console.WriteLine("Incorrect PIN code.");
        logger.Log($"Failed PIN validation for {user.FullName}");
        return null;
    }

    logger.Log($"Successful login: {user.FullName}");
    return user;
}

static void ShowMainMenu(User user, BankService bankService, ValidationService validationService)
{
    bool userIsLoggedIn = true;

    while (userIsLoggedIn)
    {
        SafeClear();
        Console.WriteLine($"Welcome, {user.FullName}");
        Console.WriteLine();
        Console.WriteLine("1. Check balance");
        Console.WriteLine("2. Withdraw money");
        Console.WriteLine("3. Show last 5 transactions");
        Console.WriteLine("4. Deposit money");
        Console.WriteLine("5. Change PIN code");
        Console.WriteLine("6. Currency conversion");
        Console.WriteLine("7. Exit / return to start");
        Console.WriteLine();
        Console.Write("Choose an option: ");

        string? choice = Console.ReadLine();

        switch (choice)
        {
            case "1":
                bankService.ShowBalances(user);
                Pause();
                break;
            case "2":
                WithdrawMoney(user, bankService, validationService);
                Pause();
                break;
            case "3":
                bankService.ShowLastFiveTransactions(user);
                Pause();
                break;
            case "4":
                DepositMoney(user, bankService, validationService);
                Pause();
                break;
            case "5":
                ChangePin(user, bankService, validationService);
                Pause();
                break;
            case "6":
                ConvertCurrency(user, bankService, validationService);
                Pause();
                break;
            case "7":
                userIsLoggedIn = false;
                break;
            default:
                Console.WriteLine("Invalid menu option. Please choose a number from 1 to 7.");
                Pause();
                break;
        }
    }
}

static void WithdrawMoney(User user, BankService bankService, ValidationService validationService)
{
    Console.WriteLine();
    Console.WriteLine("Withdraw money");

    if (!TryAskForCurrency(validationService, "Choose currency", out Currency currency))
    {
        return;
    }

    if (!TryAskForAmount("Enter amount: ", out decimal amount))
    {
        return;
    }

    if (bankService.Withdraw(user, currency, amount))
    {
        Console.WriteLine($"Successfully withdrew {amount:F2} {currency}.");
    }
}

static void DepositMoney(User user, BankService bankService, ValidationService validationService)
{
    Console.WriteLine();
    Console.WriteLine("Deposit money");

    if (!TryAskForCurrency(validationService, "Choose currency", out Currency currency))
    {
        return;
    }

    if (!TryAskForAmount("Enter amount: ", out decimal amount))
    {
        return;
    }

    if (bankService.Deposit(user, currency, amount))
    {
        Console.WriteLine($"Successfully deposited {amount:F2} {currency}.");
    }
}

static void ChangePin(User user, BankService bankService, ValidationService validationService)
{
    Console.WriteLine();
    Console.WriteLine("Change PIN");
    Console.Write("Enter old PIN: ");
    string oldPin = ReadRequiredText();

    Console.Write("Enter new PIN: ");
    string newPin = ReadRequiredText();

    if (bankService.ChangePin(user, oldPin, newPin, validationService))
    {
        Console.WriteLine("PIN changed successfully.");
    }
}

static void ConvertCurrency(User user, BankService bankService, ValidationService validationService)
{
    Console.WriteLine();
    Console.WriteLine("Currency conversion");
    Console.WriteLine("Fixed rates: 1 USD = 2.70 GEL, 1 EUR = 2.95 GEL");

    if (!TryAskForCurrency(validationService, "Choose source currency", out Currency sourceCurrency))
    {
        return;
    }

    if (!TryAskForCurrency(validationService, "Choose target currency", out Currency targetCurrency))
    {
        return;
    }

    if (!TryAskForAmount("Enter amount to convert: ", out decimal amount))
    {
        return;
    }

    bankService.ConvertCurrency(user, sourceCurrency, targetCurrency, amount);
}

static bool TryAskForCurrency(ValidationService validationService, string title, out Currency currency)
{
    Console.WriteLine();
    Console.WriteLine(title);
    Console.WriteLine("1. GEL");
    Console.WriteLine("2. USD");
    Console.WriteLine("3. EUR");
    Console.Write("Choose currency: ");

    string? input = Console.ReadLine();

    if (!validationService.TryReadCurrency(input, out currency))
    {
        Console.WriteLine("Invalid currency choice.");
        return false;
    }

    return true;
}

static bool TryAskForAmount(string message, out decimal amount)
{
    Console.Write(message);
    string? input = Console.ReadLine();

    if (!decimal.TryParse(input, out amount))
    {
        Console.WriteLine("Please enter a valid number.");
        return false;
    }

    if (amount <= 0)
    {
        Console.WriteLine("Amount must be greater than zero.");
        return false;
    }

    return true;
}

static string ReadRequiredText()
{
    string? input = Console.ReadLine();
    return input?.Trim() ?? string.Empty;
}

static void Pause()
{
    Console.WriteLine();
    Console.Write("Press Enter to continue...");
    Console.ReadLine();
}

static void SafeClear()
{
    try
    {
        Console.Clear();
    }
    catch
    {
        
    }
}

static string FindProjectFolder()
{
    DirectoryInfo? currentDirectory = new(Directory.GetCurrentDirectory());

    while (currentDirectory != null)
    {
        bool hasProjectFile = currentDirectory
            .GetFiles("*.csproj")
            .Any();

        if (hasProjectFile)
        {
            return currentDirectory.FullName;
        }

        currentDirectory = currentDirectory.Parent;
    }

    return Directory.GetCurrentDirectory();
}
