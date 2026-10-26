using System.Globalization;
using BankingSystem.Models;
using BankingSystem.Services;

namespace BankingSystem;

public class MainForm : Form
{
    private readonly BankService _bank;
    private readonly CultureInfo _money = new("en-ZA"); // formats amounts as Rands

    private readonly TextBox _searchBox = new();
    private readonly DataGridView _accountsGrid = new();
    private readonly DataGridView _transactionsGrid = new();
    private readonly Label _summaryLabel = new();
    private readonly Label _totalLabel = new();
    private bool _refreshing;

    public MainForm()
    {
        _bank = new BankService(new JsonAccountStorage());
        BuildUi();
        RefreshAccounts();
    }

    // ---------- UI construction ----------

    private void BuildUi()
    {
        Text = "Banking Management System";
        Size = new Size(1050, 620);
        MinimumSize = new Size(900, 500);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9.5f);

        // Left side: accounts list
        ConfigureGrid(_accountsGrid);
        _accountsGrid.Columns.Add("number", "Account no.");
        _accountsGrid.Columns.Add("holder", "Holder");
        _accountsGrid.Columns.Add("type", "Type");
        _accountsGrid.Columns.Add("balance", "Balance");
        _accountsGrid.Columns["balance"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _accountsGrid.SelectionChanged += (_, _) => RefreshTransactions();

        // Right side: summary, action buttons, transaction history
        ConfigureGrid(_transactionsGrid);
        _transactionsGrid.Columns.Add("date", "Date");
        _transactionsGrid.Columns.Add("type", "Type");
        _transactionsGrid.Columns.Add("description", "Description");
        _transactionsGrid.Columns.Add("amount", "Amount");
        _transactionsGrid.Columns.Add("balance", "Balance after");
        _transactionsGrid.Columns["amount"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        _transactionsGrid.Columns["balance"]!.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        _summaryLabel.Dock = DockStyle.Top;
        _summaryLabel.Height = 40;
        _summaryLabel.Font = new Font(Font, FontStyle.Bold);
        _summaryLabel.TextAlign = ContentAlignment.MiddleLeft;

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 45, Padding = new Padding(0, 5, 0, 0) };
        buttons.Controls.Add(MakeButton("New account", NewAccount_Click));
        buttons.Controls.Add(MakeButton("Deposit", Deposit_Click));
        buttons.Controls.Add(MakeButton("Withdraw", Withdraw_Click));
        buttons.Controls.Add(MakeButton("Transfer", Transfer_Click));
        buttons.Controls.Add(MakeButton("Close account", CloseAccount_Click));

        var split = new SplitContainer { Dock = DockStyle.Fill };
        split.Panel1.Controls.Add(_accountsGrid);
        // Fill control is added first; controls docked to the top are added after it.
        split.Panel2.Controls.Add(_transactionsGrid);
        split.Panel2.Controls.Add(buttons);
        split.Panel2.Controls.Add(_summaryLabel);
        Load += (_, _) => split.SplitterDistance = 460;

        // Top bar: search
        var searchLabel = new Label { Text = "Search:", Left = 10, Top = 14, AutoSize = true };
        _searchBox.SetBounds(70, 10, 280, 25);
        _searchBox.PlaceholderText = "Account number or holder name";
        _searchBox.TextChanged += (_, _) => RefreshAccounts();
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 45 };
        topPanel.Controls.Add(searchLabel);
        topPanel.Controls.Add(_searchBox);

        // Bottom bar: bank totals
        _totalLabel.Dock = DockStyle.Bottom;
        _totalLabel.Height = 30;
        _totalLabel.Padding = new Padding(10, 0, 0, 0);
        _totalLabel.TextAlign = ContentAlignment.MiddleLeft;

        Controls.Add(split);
        Controls.Add(topPanel);
        Controls.Add(_totalLabel);
    }

    private static void ConfigureGrid(DataGridView grid)
    {
        grid.Dock = DockStyle.Fill;
        grid.ReadOnly = true;
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.AllowUserToResizeRows = false;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = SystemColors.Window;
        grid.BorderStyle = BorderStyle.None;
    }

    private static Button MakeButton(string text, EventHandler onClick)
    {
        var button = new Button { Text = text, Width = 110, Height = 32 };
        button.Click += onClick;
        return button;
    }

    // ---------- Refreshing the display ----------

    private void RefreshAccounts()
    {
        var keep = _accountsGrid.CurrentRow?.Cells[0].Value as string;

        _refreshing = true;
        _accountsGrid.Rows.Clear();
        foreach (var account in _bank.Search(_searchBox.Text))
        {
            int index = _accountsGrid.Rows.Add(
                account.AccountNumber,
                account.HolderName,
                account.Type,
                account.Balance.ToString("C2", _money));

            if (account.Balance < 0)
                _accountsGrid.Rows[index].DefaultCellStyle.ForeColor = Color.Firebrick;

            if (account.AccountNumber == keep)
                _accountsGrid.CurrentCell = _accountsGrid.Rows[index].Cells[0];
        }
        _refreshing = false;

        _totalLabel.Text = $"Accounts: {_bank.Accounts.Count}    |    Total held: {_bank.TotalHoldings.ToString("C2", _money)}";
        RefreshTransactions();
    }

    private void RefreshTransactions()
    {
        if (_refreshing) return;

        _transactionsGrid.Rows.Clear();
        var account = SelectedAccount();
        if (account == null)
        {
            _summaryLabel.Text = "No account selected";
            return;
        }

        _summaryLabel.Text =
            $"{account.HolderName}  |  {account.AccountNumber}  |  {account.Type}  |  Balance: {account.Balance.ToString("C2", _money)}";

        // Newest first
        foreach (var t in account.Transactions.AsEnumerable().Reverse())
        {
            _transactionsGrid.Rows.Add(
                t.Date.ToString("yyyy-MM-dd HH:mm"),
                t.Type,
                t.Description,
                t.Amount.ToString("C2", _money),
                t.BalanceAfter.ToString("C2", _money));
        }
    }

    private Account? SelectedAccount()
    {
        var number = _accountsGrid.CurrentRow?.Cells[0].Value as string;
        return number == null ? null : _bank.Accounts.FirstOrDefault(a => a.AccountNumber == number);
    }

    private Account? RequireSelectedAccount()
    {
        var account = SelectedAccount();
        if (account == null)
            MessageBox.Show("Please select an account first.", "No account selected",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        return account;
    }

    /// <summary>Runs a bank action, shows friendly errors, and refreshes the screen.</summary>
    private void Run(Action action)
    {
        try
        {
            action();
            RefreshAccounts();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(ex.Message, "Cannot complete that action",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ---------- Button handlers ----------

    private void NewAccount_Click(object? sender, EventArgs e)
    {
        using var dialog = new NewAccountForm();
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        Run(() =>
        {
            var account = _bank.CreateAccount(dialog.HolderName, dialog.SelectedType, dialog.OpeningDeposit);
            _searchBox.Clear();
            _accountsGrid.ClearSelection();
            RefreshAccounts();
            SelectAccount(account.AccountNumber);
        });
    }

    private void SelectAccount(string accountNumber)
    {
        foreach (DataGridViewRow row in _accountsGrid.Rows)
        {
            if (row.Cells[0].Value as string == accountNumber)
            {
                _accountsGrid.CurrentCell = row.Cells[0];
                return;
            }
        }
    }

    private void Deposit_Click(object? sender, EventArgs e)
    {
        var account = RequireSelectedAccount();
        if (account == null) return;

        using var dialog = new AmountForm("Deposit", $"Amount to deposit into {account.DisplayName}:");
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        Run(() => _bank.Deposit(account.AccountNumber, dialog.Amount));
    }

    private void Withdraw_Click(object? sender, EventArgs e)
    {
        var account = RequireSelectedAccount();
        if (account == null) return;

        using var dialog = new AmountForm("Withdraw", $"Amount to withdraw from {account.DisplayName}:");
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        Run(() => _bank.Withdraw(account.AccountNumber, dialog.Amount));
    }

    private void Transfer_Click(object? sender, EventArgs e)
    {
        var account = RequireSelectedAccount();
        if (account == null) return;

        var others = _bank.Accounts.Where(a => a.AccountNumber != account.AccountNumber).ToList();
        if (others.Count == 0)
        {
            MessageBox.Show("You need at least two accounts to make a transfer.", "Transfer",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new TransferForm(account, others);
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        Run(() => _bank.Transfer(account.AccountNumber, dialog.Destination!.AccountNumber, dialog.Amount));
    }

    private void CloseAccount_Click(object? sender, EventArgs e)
    {
        var account = RequireSelectedAccount();
        if (account == null) return;

        var answer = MessageBox.Show($"Close account {account.DisplayName}? This cannot be undone.",
            "Close account", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (answer != DialogResult.Yes) return;

        Run(() => _bank.CloseAccount(account.AccountNumber));
    }
}