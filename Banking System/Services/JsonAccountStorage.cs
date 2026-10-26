using System.Text.Json;
using System.Text.Json.Serialization;
using BankingSystem.Models;

namespace BankingSystem.Services;

public class JsonAccountStorage : IAccountStorage
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _path;

    public JsonAccountStorage(string? path = null)
    {
        _path = path ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "BankingSystem",
            "accounts.json");
    }

    public List<Account> Load()
    {
        if (!File.Exists(_path))
            return new List<Account>();

        try
        {
            var json = File.ReadAllText(_path);
            return JsonSerializer.Deserialize<List<Account>>(json, Options) ?? new List<Account>();
        }
        catch (JsonException)
        {
            // Keep a copy of the unreadable file instead of silently overwriting it later.
            File.Copy(_path, _path + ".bak", overwrite: true);
            return new List<Account>();
        }
    }

    public void Save(IEnumerable<Account> accounts)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(accounts, Options));
    }
}