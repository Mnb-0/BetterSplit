using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BetterSplit.Models;

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
            cmbPayer.Items.Clear(); // Clear any previous items

            if (_currentGroup?.Members != null)
            {
                // Add members to ComboBox
                foreach (var member in _currentGroup.Members)
                {
                    cmbPayer.Items.Add(member);
                }

                // Set the DisplayMemberPath to 'Name' so it shows member names
                cmbPayer.DisplayMemberPath = "Name";

                // Set the selected index if there are items in the ComboBox
                if (cmbPayer.Items.Count > 0)
                {
                    cmbPayer.SelectedIndex = 0; // Set the first item as selected
                }

                // Update the balance display after loading payer options
                LoadBalanceDisplay();
            }
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
                null);  // No direct payee for expenses
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
                        Tag = i // Store index for validation
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
                // Get the selected member who is settling up
                var selectedMember = (Member)cmbPayer.SelectedItem;

                if (selectedMember.Balance != 0)
                {
                    // Settle the balance
                    decimal amountToSettle = selectedMember.Balance;
                    selectedMember.Balance = 0;  // Set the balance to 0 after paying

                    // Display a success message
                    ShowSuccessMessage($"Member {selectedMember.Name} has settled up the balance of {amountToSettle:C2}");
                }
                else
                {
                    ShowErrorMessage("The selected member has no outstanding balance to settle.");
                }

                // Update the balance display
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

                // Initialize the group with members
                _currentGroup = new Group
                {
                    Name = txtGroupName.Text.Trim(),
                    Members = new List<Member>()
                };

                foreach (var name in memberNames)
                {
                    _currentGroup.Members.Add(new Member(name));
                }

                // Add the group to the list
                _allGroups.Add(_currentGroup);

                // Refresh Settle Group combo box
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
            // Simply load the balance display after expenses have been updated
            LoadBalanceDisplay();
        }

        private void cmbPayer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPayer.SelectedItem is Member selectedMember)
            {
                // Display the selected member's name
                MessageBox.Show($"Selected Payer: {selectedMember.Name}");
            }
        }

        private void ClearExpenseForm()
        {
            txtExpenseName.Clear();
            txtExpenseAmount.Clear();
            cmbPayer.SelectedIndex = 0;  // Reset the payer selection to the first item
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

            // Perform settlement
            payer.Balance += amount;
            payee.Balance -= amount;

            MessageBox.Show("Payment settled successfully!");

            var transaction = new Transaction(
            $"Settlement from {payer.Name} to {payee.Name}",
            amount,
            payer,
            payee);
            _currentGroup.Transactions.Add(transaction);

            UpdateHistories();

            // Clear fields
            txtSettleAmount.Text = "";
            cmbSettlePayer.SelectedItem = null;
            cmbSettlePayee.SelectedItem = null; 

            // Update the displayed balances
            if (_currentGroup == group)
            {
                LoadBalanceDisplay();
            }
        }
        private void SplitExpenseEqually(Expense expense)
        {
            decimal share = expense.Amount / _currentGroup.Members.Count;

            foreach (var member in _currentGroup.Members)
            {
                if (member != expense.Payer)
                {
                    // Each non-payer member owes their share to the payer
                    member.Balance -= share;
                    expense.Payer.Balance += share;
                }
            }
        }
        private void UpdateHistories()
        {
            // Update expense history
            lstExpenses.ItemsSource = null;
            lstExpenses.ItemsSource = _currentGroup.Expenses;

            // Update transaction history
            lstTransactionHistory.ItemsSource = null;
            lstTransactionHistory.ItemsSource = _currentGroup.Transactions
                .OrderByDescending(t => t.Date)
                .ToList();

            // Update balances
            LoadBalanceDisplay();
        }

    }
}
