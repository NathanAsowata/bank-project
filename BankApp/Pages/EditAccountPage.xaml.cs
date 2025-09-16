using BIZ;
using DAL.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BankApp.Pages
{
    public partial class EditAccountPage : Page
    {
        private MainWindow _mainWindow;
        private AccountService _accountService = new AccountService();
        private int _accountId;
        private Account _currentAccount;

        public EditAccountPage(MainWindow mainWindow, int accountId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _accountId = accountId;
            PopulateCounties();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAccountData();
        }

        private void PopulateCounties()
        {
            CmbCounty.ItemsSource = Enum.GetValues(typeof(County)).Cast<County>();
        }

        private void LoadAccountData()
        {
            try
            {
                _currentAccount = _accountService.GetAccountById(_accountId, App.CurrentUser);
                if (_currentAccount == null)
                {
                    MessageBox.Show($"Account with ID {_accountId} not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _mainWindow.NavigateToDashboard();
                    return;
                }

                TbAccountNumber.Text = _currentAccount.AccountID.ToString();
                TbFirstName.Text = _currentAccount.FirstName;
                TbSurname.Text = _currentAccount.Surname;
                TbAccountType.Text = _currentAccount.AccountType;
                TbSortCode.Text = _currentAccount.SortCode.ToString();
                TbBalance.Text = _currentAccount.Balance.ToString("C", CultureInfo.GetCultureInfo("en-IE"));

                TxtEmail.Text = _currentAccount.Email;
                TxtPhone.Text = _currentAccount.Phone;
                TxtAddress1.Text = _currentAccount.AddressLine1;
                TxtAddress2.Text = _currentAccount.AddressLine2;
                TxtCity.Text = _currentAccount.City;

                if (Enum.TryParse<County>(_currentAccount.County, out var selectedCounty))
                {
                    CmbCounty.SelectedItem = selectedCounty;
                }
                else
                {
                    CmbCounty.SelectedIndex = 0;
                }

                bool isCurrent = _currentAccount.AccountType == "Current";
                LblOverdraft.Visibility = isCurrent ? Visibility.Visible : Visibility.Collapsed;
                TxtOverdraftLimit.Visibility = isCurrent ? Visibility.Visible : Visibility.Collapsed;
                TxtOverdraftLimit.Text = _currentAccount.OverdraftLimit.ToString("F2");
            }
            catch (Exception ex)
            {
                HandleError("Error loading account details", ex);
                _mainWindow.NavigateToDashboard();
            }
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (_currentAccount == null) return;

            if (CmbCounty.SelectedItem == null)
            {
                MessageBox.Show("Please select a County.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal overdraftLimit = _currentAccount.OverdraftLimit;
            if (_currentAccount.AccountType == "Current")
            {
                if (!decimal.TryParse(TxtOverdraftLimit.Text, out overdraftLimit) || overdraftLimit < 0)
                {
                    MessageBox.Show("Please enter a valid non-negative Overdraft Limit.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            if (!string.IsNullOrWhiteSpace(TxtEmail.Text) && !TxtEmail.Text.Contains("@"))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                // return;
            }

            var updatedAccount = new Account
            {
                AccountID = _currentAccount.AccountID,
                Email = TxtEmail.Text.Trim(),
                Phone = TxtPhone.Text.Trim(),
                AddressLine1 = TxtAddress1.Text.Trim(),
                AddressLine2 = TxtAddress2.Text.Trim(),
                City = TxtCity.Text.Trim(),
                County = CmbCounty.SelectedItem.ToString(),
                OverdraftLimit = overdraftLimit,
                FirstName = _currentAccount.FirstName,
                Surname = _currentAccount.Surname,
                AccountType = _currentAccount.AccountType,
                SortCode = _currentAccount.SortCode,
                Balance = _currentAccount.Balance
            };

            try
            {
                bool success = _accountService.UpdateAccountDetails(updatedAccount, App.CurrentUser);

                if (success)
                {
                    MessageBox.Show("Account details updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    _mainWindow.ShowStatusMessage($"Account {_accountId} updated.");
                    _mainWindow.NavigateToDashboard();
                }
                else
                {
                    MessageBox.Show("Failed to update account details. The account might not exist or an error occurred.", "Update Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                HandleError("Error saving account changes", ex);
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