using BIZ;
using DAL.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace BankApp.Pages
{
    public partial class DepositPage : Page
    {
        private MainWindow _mainWindow;
        private TransactionService _transactionService = new TransactionService();
        private AccountService _accountService = new AccountService();
        private int _accountId;
        private Account _targetAccount;

        public DepositPage(MainWindow mainWindow, int accountId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _accountId = accountId;
            TxtAmount.Focus();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAccountInfo();
        }

        private void LoadAccountInfo()
        {
            try
            {
                _targetAccount = _accountService.GetAccountById(_accountId, App.CurrentUser);
                if (_targetAccount == null)
                {
                    MessageBox.Show($"Account {_accountId} not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _mainWindow.NavigateToDashboard();
                    return;
                }

                TbAccountNumber.Text = _targetAccount.AccountID.ToString();
                TbAccountHolder.Text = _targetAccount.FullName;
                TbCurrentBalance.Text = _targetAccount.Balance.ToString("C", CultureInfo.GetCultureInfo("en-IE"));
            }
            catch (Exception ex)
            {
                HandleError("Error loading account info", ex);
                _mainWindow.NavigateToDashboard();
            }
        }

        private void Deposit_Click(object sender, RoutedEventArgs e)
        {
            if (_targetAccount == null) return;

            if (!decimal.TryParse(TxtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid positive amount to deposit.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string description = TxtDescription.Text.Trim();

            try
            {
                bool requiresApproval = (App.CurrentUser.Role == "Teller" && amount >= 10000);
                string confirmationMessage = requiresApproval
                    ? $"Deposit of {amount:C} requires manager approval. Submit?"
                    : $"Confirm deposit of {amount:C} to account {_accountId}?";

                if (MessageBox.Show(confirmationMessage, "Confirm Deposit", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bool success = _transactionService.Deposit(_accountId, amount, description, App.CurrentUser);

                    if (success)
                    {
                        string successMsg = requiresApproval
                            ? "Deposit request submitted for approval."
                            : "Deposit successful!";
                        MessageBox.Show(successMsg, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        _mainWindow.ShowStatusMessage($"Deposit action processed for account {_accountId}.");
                        _mainWindow.NavigateToDashboard();
                    }
                }
            }
            catch (Exception ex)
            {
                HandleError("Error processing deposit", ex);
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