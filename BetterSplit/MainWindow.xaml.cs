using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BetterSplit
{
    public static class UserStore
    {
        private static readonly string filePath = "users.txt";
        private static Dictionary<string, string> users = new Dictionary<string, string>();

        static UserStore()
        {
            LoadUsersFromFile();
        }

        private static void LoadUsersFromFile()
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
                return;
            }

            users.Clear();
            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                var parts = line.Split(':');
                if (parts.Length == 2)
                {
                    string username = parts[0];
                    string passwordHash = parts[1];
                    users[username] = passwordHash;
                }
            }
        }

        private static void SaveUserToFile(string username, string password)
        {
            string passwordHash = HashPassword(password);
            using (StreamWriter sw = File.AppendText(filePath))
            {
                sw.WriteLine($"{username}:{passwordHash}");
            }
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2")); // convert byte to hex string
                }
                return sb.ToString();
            }
        }

        public static bool ValidateUser(string username, string password)
        {
            string enteredPasswordHash = HashPassword(password);
            return users.TryGetValue(username, out string storedPasswordHash) && storedPasswordHash == enteredPasswordHash;
        }

        public static bool AddUser(string username, string password)
        {
            if (users.ContainsKey(username))
                return false;

            users.Add(username, HashPassword(password));
            SaveUserToFile(username, password);
            return true;
        }
    }

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Visibility = Visibility.Collapsed;
        }

        private void NavigateToDashboard()
        {
            LoginForm.Visibility = Visibility.Collapsed;
            MainFrame.Visibility = Visibility.Visible;
            MainFrame.Navigate(new DashboardPage());
        }

        private bool ValidateInputs(string username, string password)
        {
            // Username validation
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Username cannot be empty.");
                return false;
            }

            if (username.Length < 3)
            {
                MessageBox.Show("Username must be at least 3 characters long.");
                return false;
            }

            if (username.Length > 15)
            {
                MessageBox.Show("Username cannot be more than 15 characters.");
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$"))
            {
                MessageBox.Show("Username can only contain letters, numbers, and underscores (_).");
                return false;
            }

            // Password validation
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Password cannot be empty.");
                return false;
            }

            if (password.Length < 6)
            {
                MessageBox.Show("Password must be at least 6 characters long.");
                return false;
            }

            if (password.Length > 20)
            {
                MessageBox.Show("Password cannot be more than 20 characters.");
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]"))
            {
                MessageBox.Show("Password must contain at least one uppercase letter (A-Z).");
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]"))
            {
                MessageBox.Show("Password must contain at least one lowercase letter (a-z).");
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[0-9]"))
            {
                MessageBox.Show("Password must contain at least one number (0-9).");
                return false;
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[\W_]")) // \W = non-word character
            {
                MessageBox.Show("Password must contain at least one special character (!, @, #, etc.).");
                return false;
            }

            return true;
        }


        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (!ValidateInputs(username, password))
                return;

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
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (!ValidateInputs(username, password))
                return;

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
