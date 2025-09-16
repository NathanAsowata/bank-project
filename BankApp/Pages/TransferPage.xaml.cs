using BIZ;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Collections.ObjectModel;

namespace BankApp.Pages
{
    public partial class TransferPage : Page, INotifyPropertyChanged
    {
        private MainWindow _mainWindow;
        private TransactionService _transactionService = new TransactionService();
        private AccountService _accountService = new AccountService();
        private int _sourceAccountId;
        private Account _sourceAccount;

        // This holds all account numbers for internal transfers.
        private List<Account> _allOtherAccounts;
        public List<Account> AllOtherAccounts
        {
            get { return _allOtherAccounts; }
            set
            {
                _allOtherAccounts = value;
                RaisePropertyChanged(nameof(AllOtherAccounts)); // Notify UI of change
            }
        }

        // --- INotifyPropertyChanged Implementation ---

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TransferPage(MainWindow mainWindow, int sourceAccountId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _sourceAccountId = sourceAccountId;
            DataContext = this;
            TxtAmount.Focus();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSourceAccountInfo();
            LoadDestinationAccounts();
            TransferType_Changed(null, null);
        }

        private void LoadSourceAccountInfo()
        {
            try
            {
                _sourceAccount = _accountService.GetAccountById(_sourceAccountId, App.CurrentUser);
                if (_sourceAccount == null)
                {
                    MessageBox.Show($"Source Account {_sourceAccountId} not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    _mainWindow.NavigateToDashboard();
                    return;
                }

                RbExternal.IsEnabled = _sourceAccount.AccountType == "Current";
                if (_sourceAccount.AccountType == "Savings")
                {
                    RbInternal.IsChecked = true;
                }

                TbSourceAccountNumber.Text = _sourceAccount.AccountID.ToString();
                TbSourceAccountHolder.Text = _sourceAccount.FullName;
                TbSourceAvailableFunds.Text = _sourceAccount.AvailableBalance.ToString("C", CultureInfo.GetCultureInfo("en-IE"));
            }
            catch (Exception ex)
            {
                HandleError("Error loading source account info", ex);
                _mainWindow.NavigateToDashboard();
            }
        }

        // Loads accounts for the ComboBox
        private void LoadDestinationAccounts()
        {
            if (App.CurrentUser == null) return;
            try
            {
                var allAccounts = _accountService.GetAllAccounts(App.CurrentUser);
                // Filter out the source account
                AllOtherAccounts = allAccounts.Where(acc => acc.AccountID != _sourceAccountId).ToList();
                // CmbDestInternalAccount.ItemsSource is now set via binding in XAML
            }
            catch (Exception ex)
            {
                HandleError("Error loading destination accounts list", ex);
                AllOtherAccounts = new List<Account>(); // Ensure it's not null
            }
        }
        private void TransferType_Changed(object sender, RoutedEventArgs e)
        {
            // Need null checks as this can fire before controls are ready
            if (PanelInternal == null || PanelExternal == null || RbInternal == null) return;

            bool isInternal = RbInternal.IsChecked == true;
            PanelInternal.Visibility = isInternal ? Visibility.Visible : Visibility.Collapsed;
            PanelExternal.Visibility = isInternal ? Visibility.Collapsed : Visibility.Visible;

            if (isInternal)
            {
                if (TxtDestExternalSortCode != null) TxtDestExternalSortCode.Clear();
                if (TxtDestExternalAccountNo != null) TxtDestExternalAccountNo.Clear();
            }
            else
            {
                if (CmbDestInternalAccount.SelectedValue != null) CmbDestInternalAccount.SelectedIndex = -1;
            }
        }

        private void Transfer_Click(object sender, RoutedEventArgs e)
        {
            if (_sourceAccount == null) return;

            if (!decimal.TryParse(TxtAmount.Text, out decimal amount) || amount <= 0)
            {
                MessageBox.Show("Please enter a valid positive amount to transfer.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? destinationAccountId = null;
            int? destinationSortCode = null;
            string destinationExternalAccountNoStr = null;
            bool isInternal = RbInternal.IsChecked == true;

            if (isInternal)
            {
                if (!int.TryParse(CmbDestInternalAccount.SelectedValue.ToString(), out int internalDestId) | internalDestId <= 0)
                {
                    MessageBox.Show($"Please enter a valid {CmbDestInternalAccount.SelectedItem.ToString()} internal Destination Account Number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (internalDestId == _sourceAccountId)
                {
                    MessageBox.Show("Cannot transfer funds to the same account.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                destinationAccountId = internalDestId;
            }
            else
            {
                if (_sourceAccount.AccountType == "Savings")
                {
                    MessageBox.Show("Savings accounts can only perform internal transfers.", "Rule Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (!int.TryParse(TxtDestExternalSortCode.Text, out int externalSortCode) || externalSortCode <= 0 || TxtDestExternalSortCode.Text.Length != 6)
                {
                    MessageBox.Show("Please enter a valid 6-digit external Destination Sort Code.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                destinationSortCode = externalSortCode;
                destinationExternalAccountNoStr = TxtDestExternalAccountNo.Text.Trim();
                if (string.IsNullOrWhiteSpace(destinationExternalAccountNoStr) || !int.TryParse(destinationExternalAccountNoStr, out int externalAccNo) || externalAccNo <= 0)
                {
                    MessageBox.Show("Please enter a valid external Destination Account Number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            string description = TxtDescription.Text.Trim();
            bool requiresApproval = (App.CurrentUser.Role == "Teller" && amount >= 10000);

            if (!requiresApproval && _sourceAccount.AvailableBalance < amount)
            {
                MessageBox.Show("Insufficient available funds for this transfer.", "Funds Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string confirmationMessage = requiresApproval
                    ? $"Transfer of {amount:C} requires manager approval. Submit?"
                    : $"Confirm transfer of {amount:C} from account {_sourceAccountId}?";

                if (MessageBox.Show(confirmationMessage, "Confirm Transfer", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    bool success = _transactionService.Transfer(
                        _sourceAccountId,
                        destinationAccountId,
                        destinationSortCode,
                        amount,
                        description,
                        App.CurrentUser
                    );

                    if (success)
                    {
                        string successMsg = requiresApproval
                           ? "Transfer request submitted for approval."
                           : "Transfer successful!";
                        MessageBox.Show(successMsg, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        _mainWindow.ShowStatusMessage($"Transfer action processed for account {_sourceAccountId}.");
                        _mainWindow.NavigateToDashboard();
                    }
                }
            }
            catch (InvalidOperationException fundsEx)
            {
                MessageBox.Show(fundsEx.Message, "Funds Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                _mainWindow.ShowStatusMessage(fundsEx.Message, true);
                LoadSourceAccountInfo();
            }
            catch (KeyNotFoundException keyEx)
            {
                MessageBox.Show(keyEx.Message, "Account Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                _mainWindow.ShowStatusMessage(keyEx.Message, true);
            }
            catch (Exception ex)
            {
                HandleError("Error processing transfer", ex);
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