using System.Windows;
using System.Windows.Controls;
using BetterSplit.Services;
using System.Collections.Generic;

namespace BetterSplit
{

    public static class UserStore
    {
        private static Dictionary<string, string> users = new Dictionary<string, string>
        {
            { "admin", "password" }
        };

        public static bool ValidateUser(string username, string password)
        {
            return users.TryGetValue(username, out string storedPassword) && storedPassword == password;
        }

        public static bool AddUser(string username, string password)
        {
            if (users.ContainsKey(username))
                return false;

            users.Add(username, password);
            return true;
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Visibility = Visibility.Collapsed; // Hide the dashboard frame initially
        }
        private void NavigateToDashboard()
        {
            // Hide the login form
            LoginForm.Visibility = Visibility.Collapsed;

            // Show the MainFrame and navigate to the dashboard page
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(new DashboardPage());
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (UserStore.ValidateUser(username, password))
            {
                NavigateToDashboard();
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter a username and password.");
                return;
            }

            bool success = UserStore.AddUser(username, password);
            if (success)
            {
                MessageBox.Show("Signup successful. You can now log in.");
            }
            else
            {
                MessageBox.Show("Username already exists. Try a different one.");
            }
        }

    }
}
