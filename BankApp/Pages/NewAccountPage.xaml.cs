using BIZ;
using DAL.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BankApp.Pages
{
    public partial class NewAccountPage : Page
    {
        private MainWindow _mainWindow;
        private AccountService _accountService = new AccountService();

        public NewAccountPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            PopulateCounties();
            SetDefaultSortCode();
            UpdateOverdraftVisibility();
            TxtFirstName.Focus();
        }

        private void PopulateCounties()
        {
            CmbCounty.ItemsSource = Enum.GetValues(typeof(County)).Cast<County>();
            CmbCounty.SelectedIndex = 0;
        }

        private void SetDefaultSortCode()
        {
            try
            {
                TxtSortCode.Text = _accountService.GetDefaultSortCode().ToString();
            }
            catch (Exception ex)
            {
                HandleError("Error getting default sort code", ex);
                TxtSortCode.Text = "101010";
            }
        }

        private void AccountType_Changed(object sender, RoutedEventArgs e)
        {
            UpdateOverdraftVisibility();
        }

        private void UpdateOverdraftVisibility()
        {
            if (LblOverdraft == null || TxtOverdraftLimit == null || RbCurrent == null)
            {
                return;
            }

            bool isCurrent = RbCurrent.IsChecked == true;
            LblOverdraft.Visibility = isCurrent ? Visibility.Visible : Visibility.Collapsed;
            TxtOverdraftLimit.Visibility = isCurrent ? Visibility.Visible : Visibility.Collapsed;
            if (!isCurrent)
            {
                TxtOverdraftLimit.Text = "0.00";
            }
        }

        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtFirstName.Text) ||
                string.IsNullOrWhiteSpace(TxtSurname.Text) ||
                CmbCounty.SelectedItem == null)
            {
                MessageBox.Show("Please fill in all required fields (*).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TxtInitialBalance.Text, out decimal initialBalance) || initialBalance < 0)
            {
                MessageBox.Show("Please enter a valid non-negative Initial Balance.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal overdraftLimit = 0;
            if (RbCurrent.IsChecked == true)
            {
                if (!decimal.TryParse(TxtOverdraftLimit.Text, out overdraftLimit) || overdraftLimit < 0)
                {
                    MessageBox.Show("Please enter a valid non-negative Overdraft Limit for Current accounts.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            if (!string.IsNullOrWhiteSpace(TxtEmail.Text) && !TxtEmail.Text.Contains("@"))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                // return;
            }

            var newAccount = new Account
            {
                FirstName = TxtFirstName.Text.Trim(),
                Surname = TxtSurname.Text.Trim(),
                Email = TxtEmail.Text.Trim(),
                Phone = TxtPhone.Text.Trim(),
                AddressLine1 = TxtAddress1.Text.Trim(),
                AddressLine2 = TxtAddress2.Text.Trim(),
                City = TxtCity.Text.Trim(),
                County = CmbCounty.SelectedItem.ToString(),
                AccountType = RbCurrent.IsChecked == true ? "Current" : "Savings",
                SortCode = int.Parse(TxtSortCode.Text),
                Balance = initialBalance,
                OverdraftLimit = overdraftLimit
            };

            try
            {
                int newAccountId = _accountService.CreateNewAccount(newAccount, App.CurrentUser);
                MessageBox.Show($"Account created successfully!\nNew Account Number: {newAccountId}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _mainWindow.ShowStatusMessage($"Account {newAccountId} created.");
                _mainWindow.NavigateToDashboard();
            }
            catch (Exception ex)
            {
                HandleError("Error creating account", ex);
            }
        }

        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.NavigateToDashboard();
            }
        }

        private void HandleError(string context, Exception ex)
        {
            string message = $"{context}: {ex.Message}";
            _mainWindow.ShowStatusMessage(message, true);
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            System.Diagnostics.Debug.WriteLine($"{context}: {ex}");
        }
    }
}