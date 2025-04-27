using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using BetterSplit.Models;
using Microsoft.Win32;

namespace BetterSplit
{
    public partial class DashboardPage : Page
    {
        private Group _currentGroup;
        private List<Group> _allGroups = new List<Group>();

        public DashboardPage()
        {
            InitializeComponent();
            InitializeNewGroupState();
            RefreshCurrentGroupComboBox();
        }

        private void InitializeNewGroupState()
        {
            _currentGroup = new Group();
            cmbPayer.Items.Clear();
            MembersPanel.Children.Clear();
            txtGroupName.Clear();
            txtNumMembers.Clear();
        }

        private void LoadPayerOptions()
        {
            cmbPayer.Items.Clear();

            if (_currentGroup?.Members != null)
            {
                foreach (var member in _currentGroup.Members)
                {
                    cmbPayer.Items.Add(member);
                }

                cmbPayer.DisplayMemberPath = "Name";

                if (cmbPayer.Items.Count > 0)
                {
                    cmbPayer.SelectedIndex = 0;
                }

                LoadBalanceDisplay();
            }
        }

        private void RefreshCurrentGroupComboBox()
        {
            cmbCurrentGroup.ItemsSource = null;
            cmbCurrentGroup.ItemsSource = _allGroups;
            cmbCurrentGroup.DisplayMemberPath = "Name";

            if (_currentGroup != null)
            {
                cmbCurrentGroup.SelectedItem = _currentGroup;
            }
        }

        private void cmbCurrentGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCurrentGroup.SelectedItem is Group selectedGroup)
            {
                _currentGroup = selectedGroup;
                LoadGroupData();
            }
        }

        private void LoadGroupData()
        {
            if (_currentGroup == null) return;

            LoadPayerOptions();
            UpdateHistories();
            RefreshSettleGroupComboBox();
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbPayer.SelectedItem == null)
                {
                    ShowErrorMessage("Please select a payer");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtExpenseName.Text))
                {
                    ShowErrorMessage("Please enter an expense description");
                    return;
                }

                if (!decimal.TryParse(txtExpenseAmount.Text, out decimal amount) || amount <= 0)
                {
                    ShowErrorMessage("Please enter a valid positive amount");
                    return;
                }

                var payer = (Member)cmbPayer.SelectedItem;
                var expense = new Expense(txtExpenseName.Text.Trim(), amount, payer);

                SplitExpenseEqually(expense);

                _currentGroup.Expenses.Add(expense);

                var transaction = new Transaction(
                    $"Expense: {expense.Description}",
                    expense.Amount,
                    expense.Payer,
                    null);
                _currentGroup.Transactions.Add(transaction);

                UpdateBalances();
                UpdateHistories();
                ClearExpenseForm();
                ShowSuccessMessage("Expense added successfully!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"An error occurred: {ex.Message}");
            }
        }

        private void SetMembers_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!int.TryParse(txtNumMembers.Text, out int memberCount) || memberCount <= 0)
                {
                    ShowErrorMessage("Please enter a valid number of members");
                    return;
                }

                MembersPanel.Children.Clear();

                for (int i = 0; i < memberCount; i++)
                {
                    var stackPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };

                    stackPanel.Children.Add(new TextBlock
                    {
                        Text = $"Member {i + 1} Name:",
                        Foreground = Brushes.White,
                        FontSize = 14
                    });

                    var textBox = new TextBox
                    {
                        Name = $"txtMember_{i}",
                        Width = 300,
                        Padding = new Thickness(10),
                        Background = new SolidColorBrush(Color.FromRgb(45, 50, 80)),
                        Foreground = Brushes.White,
                        BorderBrush = Brushes.Gray,
                        FontSize = 14,
                        Tag = i
                    };

                    stackPanel.Children.Add(textBox);
                    MembersPanel.Children.Add(stackPanel);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error setting members: {ex.Message}");
            }
        }

        private void SettleUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedMember = (Member)cmbPayer.SelectedItem;

                if (selectedMember.Balance != 0)
                {
                    decimal amountToSettle = selectedMember.Balance;
                    selectedMember.Balance = 0;

                    ShowSuccessMessage($"Member {selectedMember.Name} has settled up the balance of {amountToSettle:C2}");
                }
                else
                {
                    ShowErrorMessage("The selected member has no outstanding balance to settle.");
                }

                LoadBalanceDisplay();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"An error occurred while settling up: {ex.Message}");
            }
        }

        private void CreateGroup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtGroupName.Text))
                {
                    ShowErrorMessage("Please enter a group name");
                    return;
                }

                var memberNames = new List<string>();
                foreach (var child in MembersPanel.Children)
                {
                    if (child is StackPanel panel && panel.Children[1] is TextBox textBox)
                    {
                        var name = textBox.Text.Trim();
                        if (string.IsNullOrEmpty(name))
                        {
                            ShowErrorMessage("All members must have names");
                            return;
                        }
                        memberNames.Add(name);
                    }
                }

                if (memberNames.Count == 0)
                {
                    ShowErrorMessage("Please set members first");
                    return;
                }

                _currentGroup = new Group
                {
                    Name = txtGroupName.Text.Trim(),
                    Members = new List<Member>(),
                    Expenses = new List<Expense>(),
                    Transactions = new List<Transaction>()
                };

                foreach (var name in memberNames)
                {
                    _currentGroup.Members.Add(new Member(name));
                }

                _allGroups.Add(_currentGroup);

                RefreshCurrentGroupComboBox();
                RefreshSettleGroupComboBox();

                LoadPayerOptions();
                ShowSuccessMessage($"Group '{_currentGroup.Name}' created with {_currentGroup.Members.Count} members!");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Error creating group: {ex.Message}");
            }
        }

        private void RefreshSettleGroupComboBox()
        {
            cmbSettleGroup.ItemsSource = null;
            cmbSettleGroup.ItemsSource = _allGroups;
            cmbSettleGroup.DisplayMemberPath = "Name";
        }

        private void LoadBalanceDisplay()
        {
            lstBalances.Items.Clear();

            foreach (var member in _currentGroup.Members)
            {
                lstBalances.Items.Add(new
                {
                    MemberName = member.Name,
                    Balance = member.Balance
                });
            }
        }

        private void UpdateBalances()
        {
            LoadBalanceDisplay();
        }

        private void cmbPayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPayer.SelectedItem is Member selectedMember)
            {
                MessageBox.Show($"Selected Payer: {selectedMember.Name}");
            }
        }

        private void ClearExpenseForm()
        {
            txtExpenseName.Clear();
            txtExpenseAmount.Clear();
            cmbPayer.SelectedIndex = 0;
        }

        private void ShowSuccessMessage(string message)
        {
            MessageBox.Show(message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void cmbSettleGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedGroup = cmbSettleGroup.SelectedItem as Group;
            if (selectedGroup != null)
            {
                cmbSettlePayer.ItemsSource = selectedGroup.Members;
                cmbSettlePayer.DisplayMemberPath = "Name";

                cmbSettlePayee.ItemsSource = selectedGroup.Members;
                cmbSettlePayee.DisplayMemberPath = "Name";
            }
        }

        private void SettlePayment_Click(object sender, RoutedEventArgs e)
        {
            var group = cmbSettleGroup.SelectedItem as Group;
            var payer = cmbSettlePayer.SelectedItem as Member;
            var payee = cmbSettlePayee.SelectedItem as Member;

            if (group == null || payer == null || payee == null)
            {
                MessageBox.Show("Please select group, payer, and payee.");
                return;
            }

            if (payer == payee)
            {
                MessageBox.Show("Payer and payee cannot be the same.");
                return;
            }

            if (!decimal.TryParse(txtSettleAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid amount.");
                return;
            }

            payer.Balance += amount;
            payee.Balance -= amount;

            var transaction = new Transaction(
                $"Settlement from {payer.Name} to {payee.Name}",
                amount,
                payer,
                payee);

            group.Transactions.Add(transaction);

            if (_currentGroup == group)
            {
                UpdateHistories();
            }

            txtSettleAmount.Text = "";
            cmbSettlePayer.SelectedItem = null;
            cmbSettlePayee.SelectedItem = null;

            MessageBox.Show("Payment settled successfully!");
        }

        private void SplitExpenseEqually(Expense expense)
        {
            decimal share = expense.Amount / _currentGroup.Members.Count;

            foreach (var member in _currentGroup.Members)
            {
                if (member != expense.Payer)
                {
                    member.Balance -= share;
                    expense.Payer.Balance += share;
                }
            }
        }

        private void UpdateHistories()
        {
            lstExpenses.ItemsSource = null;
            lstExpenses.ItemsSource = _currentGroup.Expenses;

            lstTransactionHistory.ItemsSource = null;
            lstTransactionHistory.ItemsSource = _currentGroup.Transactions
                .OrderByDescending(t => t.Date)
                .ToList();

            LoadBalanceDisplay();
        }

        private void ExportToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                DefaultExt = ".csv",
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                try
                {
                    ExportDataToCsv(filePath);
                    MessageBox.Show("Data exported successfully!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportDataToCsv(string filePath)
        {
            StringBuilder csvContent = new StringBuilder();

            // Add group metadata
            csvContent.AppendLine($"Group Name: {_currentGroup.Name}");
            csvContent.AppendLine($"Member Count: {_currentGroup.Members.Count}");
            csvContent.AppendLine("Description,Amount,Payer,Member,Share");

            // Export Expenses and Shares
            foreach (var expense in _currentGroup.Expenses)
            {
                decimal share = expense.Amount / _currentGroup.Members.Count;
                foreach (var member in _currentGroup.Members)
                {
                    csvContent.AppendLine($"{EscapeCsv(expense.Description)},{expense.Amount},{EscapeCsv(expense.Payer.Name)},{EscapeCsv(member.Name)},{share}");
                }
            }

            File.WriteAllText(filePath, csvContent.ToString(), Encoding.UTF8);
        }

        private void ImportFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV Files (*.csv)|*.csv",
                DefaultExt = ".csv"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    ImportDataFromCsv(filePath);
                    MessageBox.Show("Data imported successfully!", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error importing data: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ImportDataFromCsv(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length == 0) return;

            string groupName = "Imported Group";
            int memberCount = 0;

            // Parse metadata lines
            if (lines[0].StartsWith("Group Name:"))
            {
                groupName = lines[0].Substring("Group Name:".Length).Trim();
                lines = lines.Skip(1).ToArray();
            }

            if (lines[0].StartsWith("Member Count:"))
            {
                int.TryParse(lines[0].Substring("Member Count:".Length).Trim(), out memberCount);
                lines = lines.Skip(1).ToArray();
            }

            // Skip header row
            var dataRows = lines.Skip(1).ToList();

            InitializeNewGroupState();
            _currentGroup = new Group
            {
                Name = groupName,
                Expenses = new List<Expense>(),
                Transactions = new List<Transaction>()
            };

            var expenseGroups = dataRows
                .Select(row => row.Split(','))
                .GroupBy(fields => fields[0]);

            var firstExpenseData = expenseGroups.First().First();
            var memberNames = expenseGroups.First()
                .Select(fields => fields[3])
                .Distinct()
                .ToList();

            _currentGroup.Members = memberNames.Select(name => new Member(name)).ToList();

            foreach (var expenseGroup in expenseGroups)
            {
                var firstRow = expenseGroup.First();
                decimal amount = decimal.Parse(firstRow[1]);
                string payerName = firstRow[2].Trim('"');
                var payer = _currentGroup.Members.First(m => m.Name == payerName);

                var expense = new Expense(firstRow[0], amount, payer);
                _currentGroup.Expenses.Add(expense);

                var transaction = new Transaction(
                    $"Expense: {expense.Description}",
                    expense.Amount,
                    expense.Payer,
                    null);
                _currentGroup.Transactions.Add(transaction);
            }

            foreach (var expense in _currentGroup.Expenses)
            {
                SplitExpenseEqually(expense);
            }

            _allGroups.Add(_currentGroup);
            RefreshCurrentGroupComboBox();
            RefreshSettleGroupComboBox();

            txtGroupName.Text = _currentGroup.Name;
            txtNumMembers.Text = _currentGroup.Members.Count.ToString();
            LoadPayerOptions();
            UpdateHistories();
        }

        private string EscapeCsv(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.Contains(",")) return $"\"{input}\"";
            return input;
        }

        private string UnescapeCsv(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.StartsWith("\"") && input.EndsWith("\""))
            {
                return input.Substring(1, input.Length - 2);
            }
            return input;
        }
    }
}