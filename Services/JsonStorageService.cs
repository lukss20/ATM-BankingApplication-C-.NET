using System.Text.Json;
using System.Text.Json.Serialization;
using BankingApplication.Models;

namespace BankingApplication.Services;

public class JsonStorageService
{
    private readonly string _filePath;
    private readonly LoggerService _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonStorageService(string filePath, LoggerService logger)
    {
        _filePath = filePath;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public List<User> LoadUsers()
    {
        try
        {
            EnsureDataFileExists();
            string json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<User>>(json, _jsonOptions) ?? new List<User>();
        }
        catch (Exception ex)
        {
            _logger.Log($"Error loading users: {ex.Message}");
            return new List<User>();
        }
    }

    public bool SaveUsers(List<User> users)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            string json = JsonSerializer.Serialize(users, _jsonOptions);
            File.WriteAllText(_filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Log($"Error saving users: {ex.Message}");
            return false;
        }
    }

    private void EnsureDataFileExists()
    {
        if (File.Exists(_filePath))
        {
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

        List<User> sampleUsers = new()
        {
            new User
            {
                FirstName = "Luka",
                LastName = "Chitaia",
                CardDetails = new CardDetails
                {
                    CardNumber = "1234567812345678",
                    ExpirationDate = "12/30",
                    CVC = "123"
                },
                PinCode = "1234",
                Balances = new Balance
                {
                    GEL = 1500,
                    USD = 500,
                    EUR = 300
                },
                TransactionHistory = new List<Transaction>()
            }
        };

        SaveUsers(sampleUsers);
    }
}
