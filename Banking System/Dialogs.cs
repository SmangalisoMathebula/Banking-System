using System.Globalization;
using BankingSystem.Models;

namespace BankingSystem;

internal static class InputHelper
{
    public static bool TryParseAmount(string? text, out decimal amount) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out amount);

    public static void ConfigureDialog(Form form, string title, Size clientSize)
    {
        form.Text = title;
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.MaximizeBox = false;
        form.MinimizeBox = false;
        form.ShowInTaskbar = false;
        form.StartPosition = FormStartPosition.CenterParent;
        form.ClientSize = clientSize;
    }
}

/// <summary>Asks for a single amount (used for deposits and withdrawals).</summary>
public class AmountForm : Form
{
    private readonly TextBox _amountBox = new();

    public decimal Amount { get; private set; }

    public AmountForm(string title, string prompt)
    {
        InputHelper.ConfigureDialog(this, title, new Size(340, 135));

        var label = new Label { Text = prompt, Left = 15, Top = 15, Width = 310, Height = 40 };
        _amountBox.SetBounds(15, 60, 310, 25);
        var ok = new Button { Text = "OK", Left = 165, Top = 97, Width = 75 };
        var cancel = new Button { Text = "Cancel", Left = 250, Top = 97, Width = 75, DialogResult = DialogResult.Cancel };

        ok.Click += OkClicked;
        AcceptButton = ok;
        CancelButton = cancel;
        Controls.AddRange(new Control[] { label, _amountBox, ok, cancel });
    }

    private void OkClicked(object? sender, EventArgs e)
    {
        if (!InputHelper.TryParseAmount(_amountBox.Text, out var amount) || amount <= 0)
        {
            MessageBox.Show("Enter a valid amount greater than zero.", "Invalid amount",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _amountBox.Focus();
            return;
        }

        Amount = amount;
        DialogResult = DialogResult.OK;
    }
}

/// <summary>Collects the details for a new account.</summary>
public class NewAccountForm : Form
{
    private readonly TextBox _nameBox = new();
    private readonly ComboBox _typeBox = new();
    private readonly TextBox _depositBox = new();

    public string HolderName { get; private set; } = "";
    public AccountType SelectedType { get; private set; }
    public decimal OpeningDeposit { get; private set; }

    public NewAccountForm()
    {
        InputHelper.ConfigureDialog(this, "New account", new Size(340, 240));

        var nameLabel = new Label { Text = "Account holder name", Left = 15, Top = 15, Width = 310 };
        _nameBox.SetBounds(15, 38, 310, 25);

        var typeLabel = new Label { Text = "Account type", Left = 15, Top = 75, Width = 310 };
        _typeBox.SetBounds(15, 98, 310, 25);
        _typeBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _typeBox.DataSource = Enum.GetValues<AccountType>();

        var depositLabel = new Label { Text = "Opening deposit (optional)", Left = 15, Top = 135, Width = 310 };
        _depositBox.SetBounds(15, 158, 310, 25);

        var ok = new Button { Text = "Create", Left = 165, Top = 198, Width = 75 };
        var cancel = new Button { Text = "Cancel", Left = 250, Top = 198, Width = 75, DialogResult = DialogResult.Cancel };

        ok.Click += OkClicked;
        AcceptButton = ok;
        CancelButton = cancel;
        Controls.AddRange(new Control[] { nameLabel, _nameBox, typeLabel, _typeBox, depositLabel, _depositBox, ok, cancel });
    }

    private void OkClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_nameBox.Text))
        {
            MessageBox.Show("Please enter the account holder's name.", "Missing name",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _nameBox.Focus();
            return;
        }

        decimal deposit = 0;
        if (!string.IsNullOrWhiteSpace(_depositBox.Text) &&
            (!InputHelper.TryParseAmount(_depositBox.Text, out deposit) || deposit < 0))
        {
            MessageBox.Show("The opening deposit must be a valid amount (or leave it blank).", "Invalid deposit",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _depositBox.Focus();
            return;
        }

        HolderName = _nameBox.Text.Trim();
        SelectedType = (AccountType)_typeBox.SelectedItem!;
        OpeningDeposit = deposit;
        DialogResult = DialogResult.OK;
    }
}

/// <summary>Picks a destination account and an amount for a transfer.</summary>
public class TransferForm : Form
{
    private readonly ComboBox _destinationBox = new();
    private readonly TextBox _amountBox = new();

    public Account? Destination { get; private set; }
    public decimal Amount { get; private set; }

    public TransferForm(Account source, IList<Account> destinations)
    {
        InputHelper.ConfigureDialog(this, "Transfer", new Size(340, 195));

        var fromLabel = new Label { Text = $"From: {source.DisplayName}", Left = 15, Top = 15, Width = 310 };

        var toLabel = new Label { Text = "To account", Left = 15, Top = 45, Width = 310 };
        _destinationBox.SetBounds(15, 68, 310, 25);
        _destinationBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _destinationBox.DataSource = destinations;
        _destinationBox.DisplayMember = nameof(Account.DisplayName);

        var amountLabel = new Label { Text = "Amount", Left = 15, Top = 103, Width = 310 };
        _amountBox.SetBounds(15, 126, 310, 25);

        var ok = new Button { Text = "Transfer", Left = 165, Top = 160, Width = 75 };
        var cancel = new Button { Text = "Cancel", Left = 250, Top = 160, Width = 75, DialogResult = DialogResult.Cancel };

        ok.Click += OkClicked;
        AcceptButton = ok;
        CancelButton = cancel;
        Controls.AddRange(new Control[] { fromLabel, toLabel, _destinationBox, amountLabel, _amountBox, ok, cancel });
    }

    private void OkClicked(object? sender, EventArgs e)
    {
        if (!InputHelper.TryParseAmount(_amountBox.Text, out var amount) || amount <= 0)
        {
            MessageBox.Show("Enter a valid amount greater than zero.", "Invalid amount",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _amountBox.Focus();
            return;
        }

        Destination = (Account)_destinationBox.SelectedItem!;
        Amount = amount;
        DialogResult = DialogResult.OK;
    }
}