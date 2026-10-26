namespace BankingSystem.Models;

public enum AccountType
{
    Savings,
    Cheque
}

public enum TransactionType
{
    Deposit,
    Withdrawal,
    TransferIn,
    TransferOut
}

public class Transaction
{
    public DateTime Date { get; set; } = DateTime.Now;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = "";
}