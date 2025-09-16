using BIZ;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // For ObservableCollection
using System.ComponentModel; // For INotifyPropertyChanged
using System.Windows;
using System.Windows.Controls;


namespace BankApp.Pages
{
    public partial class DashboardPage : Page, INotifyPropertyChanged
    {
        private AccountService _accountService = new AccountService();
        private TransactionService _transactionService = new TransactionService();
        private MainWindow _mainWindow;

        // Use ObservableCollection for automatic UI updates when items are added/removed
        private ObservableCollection<Account> _accounts;
        public ObservableCollection<Account> Accounts
        {
            get { return _accounts; }
            set { _accounts = value; OnPropertyChanged(nameof(Accounts)); }
        }

        private ObservableCollection<TransactionLog> _pendingTransactions;
        public ObservableCollection<TransactionLog> PendingTransactions
        {
            get { return _pendingTransactions; }
            set { _pendingTransactions = value; OnPropertyChanged(nameof(PendingTransactions)); }
        }


        public DashboardPage(MainWindow mainWindow, bool refreshData = true)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            DataContext = this; // Set DataContext for binding

            if (refreshData)
            {
                LoadAccountsData();
                SetupManagerAdminView();
            }
        }

        public void LoadAccountsData()
        {
            if (App.CurrentUser == null) return;

            try
            {
                List<Account> accountList = _accountService.GetAllAccounts(App.CurrentUser);
                Accounts = new ObservableCollection<Account>(accountList); // Update the collection
                _mainWindow.ShowStatusMessage($"Loaded {Accounts.Count} accounts.");
            }
            catch (Exception ex)
            {
                HandleError("Error loading accounts", ex);
            }
        }

        private void SetupManagerAdminView()
        {
            if (App.CurrentUser != null && (App.CurrentUser.Role == "Manager" || App.CurrentUser.Role == "Admin"))
            {
                PendingTitle.Visibility = Visibility.Visible;
                PendingTransactionsGrid.Visibility = Visibility.Visible;
                ApprovalButtonsPanel.Visibility = Visibility.Visible;
                LoadPendingTransactionsData();
            }
            else
            {
                PendingTitle.Visibility = Visibility.Collapsed;
                PendingTransactionsGrid.Visibility = Visibility.Collapsed;
                ApprovalButtonsPanel.Visibility = Visibility.Collapsed;
            }
        }

        public void LoadPendingTransactionsData()
        {
            if (App.CurrentUser == null || (App.CurrentUser.Role != "Manager" && App.CurrentUser.Role != "Admin")) return;

            try
            {
                List<TransactionLog> pendingList = _transactionService.GetPendingApprovalTransactions(App.CurrentUser);
                PendingTransactions = new ObservableCollection<TransactionLog>(pendingList);
                _mainWindow.ShowStatusMessage($"Loaded {PendingTransactions.Count} pending transactions.");
            }
            catch (Exception ex)
            {
                HandleError("Error loading pending transactions", ex);
            }
        }


        private void BtnApprove_Click(object sender, RoutedEventArgs e)
        {
            ProcessApprovalSelection("Approved");
        }

        private void BtnReject_Click(object sender, RoutedEventArgs e)
        {
            ProcessApprovalSelection("Rejected");
        }

        private void ProcessApprovalSelection(string action) // action = "Approved" or "Rejected"
        {
            if (PendingTransactionsGrid.SelectedItem is TransactionLog selectedTransaction)
            {
                if (MessageBox.Show($"Are you sure you want to {action.ToLower()} transaction ID {selectedTransaction.TransactionID}?",
                                    $"{action} Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = false;
                        if (action == "Approved")
                        {
                            success = _transactionService.ApproveTransaction(selectedTransaction.TransactionID, App.CurrentUser);
                        }
                        else // Rejected
                        {
                            success = _transactionService.RejectTransaction(selectedTransaction.TransactionID, App.CurrentUser);
                        }

                        if (success)
                        {
                            _mainWindow.ShowStatusMessage($"Transaction {selectedTransaction.TransactionID} has been {action.ToLower()}.");
                            LoadPendingTransactionsData(); // Refresh pending list
                            LoadAccountsData(); // Refresh account balances as approval might change them
                        }
                        else
                        {
                            // Error handling incase the BIZ layer fails to catch the error
                            MessageBox.Show($"Failed to {action.ToLower()} the transaction. It might have been processed already.", "Processing Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            LoadPendingTransactionsData(); // Refresh the list
                        }
                    }
                    catch (Exception ex)
                    {
                        HandleError($"Error {action.ToLower()}ing transaction", ex);
                        LoadPendingTransactionsData(); // Refresh list after error
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a transaction from the pending list first.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void HandleError(string context, Exception ex)
        {
            string message = $"{context}: {ex.Message}";
            _mainWindow.ShowStatusMessage(message, true);
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            System.Diagnostics.Debug.WriteLine($"{context}: {ex}"); // Log full error
        }


        // INotifyPropertyChanged implemented
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
