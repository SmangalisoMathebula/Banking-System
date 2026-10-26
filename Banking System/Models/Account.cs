using System.Text.Json.Serialization;

namespace BankingSystem.Models;

public class Account
{
    /// <summary>Cheque accounts may go this far into the red. Savings accounts may not.</summary>
    public const decimal ChequeOverdraftLimit = 500m;

    public string AccountNumber { get; set; } = "";
    public string HolderName { get; set; } = "";
    public AccountType Type { get; set; }
    public decimal Balance { get; set; }
    public DateTime Opened { get; set; } = DateTime.Now;
    public List<Transaction> Transactions { get; set; } = new();

    [JsonIgnore]
    public decimal OverdraftLimit => Type == AccountType.Cheque ? ChequeOverdraftLimit : 0m;

    [JsonIgnore]
    public string DisplayName => $"{AccountNumber} - {HolderName}";

    /// <summary>Adds money to the account and records the transaction.</summary>
    public void Credit(decimal amount, TransactionType type = TransactionType.Deposit, string description = "Deposit")
    {
        RequireValidAmount(amount);
        Balance += amount;
        Record(type, amount, description);
    }

    /// <summary>Removes money from the account (respecting the overdraft limit) and records the transaction.</summary>
    public void Debit(decimal amount, TransactionType type = TransactionType.Withdrawal, string description = "Withdrawal")
    {
        RequireValidAmount(amount);
        if (amount > Balance + OverdraftLimit)
            throw new InvalidOperationException("Insufficient funds.");

        Balance -= amount;
        Record(type, amount, description);
    }

    private static void RequireValidAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");
        if (decimal.Round(amount, 2) != amount)
            throw new ArgumentException("Amount cannot have more than two decimal places.");
    }

    private void Record(TransactionType type, decimal amount, string description)
    {
        Transactions.Add(new Transaction
        {
            Date = DateTime.Now,
            Type = type,
            Amount = amount,
            BalanceAfter = Balance,
            Description = description
        });
    }
}