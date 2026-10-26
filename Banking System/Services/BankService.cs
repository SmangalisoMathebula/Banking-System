using BankingSystem.Models;

namespace BankingSystem.Services;

/// <summary>All the banking rules live here. The forms only call these methods.</summary>
public class BankService
{
    private readonly IAccountStorage _storage;
    private readonly List<Account> _accounts;

    public BankService(IAccountStorage storage)
    {
        _storage = storage;
        _accounts = _storage.Load();
    }

    public IReadOnlyList<Account> Accounts => _accounts;

    public decimal TotalHoldings => _accounts.Sum(a => a.Balance);

    public Account CreateAccount(string holderName, AccountType type, decimal openingDeposit = 0m)
    {
        if (string.IsNullOrWhiteSpace(holderName))
            throw new ArgumentException("Account holder name is required.");
        if (openingDeposit < 0)
            throw new ArgumentException("Opening deposit cannot be negative.");

        var account = new Account
        {
            AccountNumber = NextAccountNumber(),
            HolderName = holderName.Trim(),
            Type = type
        };

        if (openingDeposit > 0)
            account.Credit(openingDeposit, TransactionType.Deposit, "Opening deposit");

        _accounts.Add(account);
        Save();
        return account;
    }

    public void Deposit(string accountNumber, decimal amount)
    {
        GetAccount(accountNumber).Credit(amount);
        Save();
    }

    public void Withdraw(string accountNumber, decimal amount)
    {
        GetAccount(accountNumber).Debit(amount);
        Save();
    }

    public void Transfer(string fromNumber, string toNumber, decimal amount)
    {
        if (fromNumber == toNumber)
            throw new InvalidOperationException("Cannot transfer to the same account.");

        var from = GetAccount(fromNumber);
        var to = GetAccount(toNumber);

        from.Debit(amount, TransactionType.TransferOut, $"Transfer to {to.AccountNumber}");
        to.Credit(amount, TransactionType.TransferIn, $"Transfer from {from.AccountNumber}");
        Save();
    }

    public void CloseAccount(string accountNumber)
    {
        var account = GetAccount(accountNumber);
        if (account.Balance != 0)
            throw new InvalidOperationException("An account can only be closed when its balance is zero.");

        _accounts.Remove(account);
        Save();
    }

    public IEnumerable<Account> Search(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return _accounts;

        text = text.Trim();
        return _accounts.Where(a =>
            a.AccountNumber.Contains(text, StringComparison.OrdinalIgnoreCase) ||
            a.HolderName.Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    private Account GetAccount(string accountNumber) =>
        _accounts.FirstOrDefault(a => a.AccountNumber == accountNumber)
        ?? throw new InvalidOperationException("Account not found.");

    private string NextAccountNumber()
    {
        long next = _accounts.Count == 0
            ? 1000000001
            : _accounts.Max(a => long.Parse(a.AccountNumber)) + 1;
        return next.ToString();
    }

    private void Save() => _storage.Save(_accounts);
}