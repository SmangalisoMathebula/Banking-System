 Banking Management System

A Windows desktop banking application I built with C# and Windows Forms. It lets you create savings and cheque accounts, make deposits and withdrawals, transfer money between accounts, and view a full transaction history for each account.

I built this project to practise object-oriented programming in C# and to learn how to structure a desktop application properly, with the user interface kept separate from the business logic.

![Main window](screenshots/main-window.png)

 Features

- Create **savings** and **cheque** accounts, with an optional opening deposit
- **Deposit**, **withdraw** and **transfer** money between accounts
- Cheque accounts have a **R500 overdraft limit**; savings accounts cannot go below zero
- **Transaction history** for every account, shown newest first
- **Search** for accounts by account number or holder name
- Accounts are **saved automatically** to a JSON file and loaded again when the app starts
- **Input validation**: invalid amounts, empty names and insufficient funds all show a clear message instead of crashing
- An account can only be closed once its balance is zero

## Screenshots

 Accounts and transaction history | Creating an account |
 ![Main window](screenshots/main-window.png) | ![New account](screenshots/new-account.png)

## Built with

- C# and .NET 8
- Windows Forms
- System.Text.Json (for saving and loading data)

## How to run it

1. Install [Visual Studio 2022](https://visualstudio.microsoft.com/) with the **.NET desktop development** workload.
2. Clone the repository:
   ```
   git clone https://github.com/SmangalisoMathebula/Banking-System.git
   ```
3. Open the `.slnx` file in Visual Studio and press **F5** to run.

Account data is saved to `%AppData%\BankingSystem\accounts.json`.

## How the code is organised

```
Models/      Account and Transaction classes, including the account rules
Services/    BankService (the banking logic) and the JSON storage
MainForm.cs  The main window
Dialogs.cs   Pop-up windows for new accounts, amounts and transfers
Program.cs   Entry point
```

The forms do not contain any banking rules. They only call `BankService`, which handles creating accounts, deposits, withdrawals, transfers and closing accounts. Saving sits behind an interface (`IAccountStorage`), so the JSON file could be replaced with a database later without changing the rest of the code.



 Author

Smangaliso Mathebula
