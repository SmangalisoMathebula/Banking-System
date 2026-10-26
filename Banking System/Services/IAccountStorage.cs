using BankingSystem.Models;

namespace BankingSystem.Services;

/// <summary>Abstraction over where accounts are stored, so the logic can be tested without touching disk.</summary>
public interface IAccountStorage
{
    List<Account> Load();
    void Save(IEnumerable<Account> accounts);
}