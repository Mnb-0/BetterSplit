# BetterSplit

A Splitwise-style desktop app for tracking shared expenses within a group, written in **C# / WPF (.NET 8)**.

## Features

- **Accounts:** sign up and log in. Passwords are stored as SHA-256 hashes in a local `users.txt`.
- **Groups:** create a group and set its members.
- **Expenses:** record who paid and how much. Each expense is split equally across the group, and every member's running balance is updated.
- **Group summary:** shows each member's balance (positive means they're owed, negative means they owe), the expense history, and the transaction history.
- **Settle up:** record a payment from one member to another, which adjusts both balances and is logged as a transaction.
- **CSV import/export:** save a group's expenses and per-member shares to a CSV file and load them back.

## How balances work

When member *P* pays amount *A* in a group of *n* people, each of the other *n − 1* members' balances goes down by *A / n*, and *P*'s goes up by the same total. A settlement of *x* from *X* to *Y* adds *x* to *X*'s balance and subtracts it from *Y*'s. All balances in a group always sum to zero.

## Project structure

```
BetterSplit/
  MainWindow.xaml(.cs)     login and sign-up, user store
  DashboardPage.xaml(.cs)  tabs: Add Group, Add Expense, Group Summary, Settle Up
  Models/                  User, Group, Member, Expense, Transaction
  Services/BudgetService.cs
```

## Building

Requires Windows and the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
dotnet run --project BetterSplit
```

Or open `BetterSplit.sln` in Visual Studio 2022.

## Limitations

- Group and expense data is held in memory. Use CSV export to keep it between sessions.
- Expenses can only be split equally; custom shares aren't supported.
- Password hashes are unsalted SHA-256. That's fine for a demo, but a real app should use a salted KDF such as PBKDF2, bcrypt or Argon2.
