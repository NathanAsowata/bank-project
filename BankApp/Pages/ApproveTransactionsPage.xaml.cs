using BIZ;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation; 

namespace BankApp.Pages
{
    public partial class ApproveTransactionsPage : Page, INotifyPropertyChanged
    {
        private MainWindow _mainWindow;
        private TransactionService _transactionService = new TransactionService();

        private ObservableCollection<TransactionLog> _pendingTransactions;
        public ObservableCollection<TransactionLog> PendingTransactions
        {
            get { return _pendingTransactions; }
            set { _pendingTransactions = value; OnPropertyChanged(nameof(PendingTransactions)); }
        }

        public ApproveTransactionsPage(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            DataContext = this; // Set DataContext for binding
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUser == null || (App.CurrentUser.Role != "Manager" && App.CurrentUser.Role != "Admin"))
            {
                MessageBox.Show("Access denied. Only Managers or Administrators can approve transactions.", "Permission Error", MessageBoxButton.OK, MessageBoxImage.Stop);
                _mainWindow.NavigateToDashboard();
                return;
            }
            LoadPendingTransactionsData();
        }

        private void LoadPendingTransactionsData()
        {
            try
            {
                List<TransactionLog> pendingList = _transactionService.GetPendingApprovalTransactions(App.CurrentUser);
                PendingTransactions = new ObservableCollection<TransactionLog>(pendingList);
                _mainWindow.ShowStatusMessage($"Loaded {PendingTransactions.Count} pending transactions for approval.");
            }
            catch (Exception ex)
            {
                HandleError("Error loading pending transactions", ex);
                PendingTransactions = new ObservableCollection<TransactionLog>();
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadPendingTransactionsData();
        }

        private void BtnApprove_Click(object sender, RoutedEventArgs e)
        {
            ProcessApprovalSelection("Approved");
        }

        private void BtnReject_Click(object sender, RoutedEventArgs e)
        {
            ProcessApprovalSelection("Rejected");
        }


        // Approve selected tranasction function
        private void ProcessApprovalSelection(string action)
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
                        else
                        {
                            success = _transactionService.RejectTransaction(selectedTransaction.TransactionID, App.CurrentUser);
                        }

                        if (success)
                        {
                            _mainWindow.ShowStatusMessage($"Transaction {selectedTransaction.TransactionID} has been {action.ToLower()}.");
                            MessageBox.Show($"Transaction {selectedTransaction.TransactionID} has been successfully {action.ToLower()}.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadPendingTransactionsData();
                        }
                        else
                        {
                            MessageBox.Show($"Failed to {action.ToLower()} the transaction. It might have been processed already or another error occurred.", "Processing Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            LoadPendingTransactionsData();
                        }
                    }
                    catch (InvalidOperationException opEx)
                    {
                        HandleError($"Error {action.ToLower()}ing transaction", opEx);
                        LoadPendingTransactionsData();
                    }
                    catch (Exception ex)
                    {
                        HandleError($"Error {action.ToLower()}ing transaction", ex);
                        LoadPendingTransactionsData();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a transaction from the list first.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // Back to Dashboard function
        private void BackToDashboard_Click(object sender, RoutedEventArgs e)
        {
            if (_mainWindow != null)
            {
                _mainWindow.NavigateToDashboard();
            }
        }

        // Handle error messages
        private void HandleError(string context, Exception ex)
        {
            string message = $"{context}: {ex.Message}";
            _mainWindow.ShowStatusMessage(message, true);
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            System.Diagnostics.Debug.WriteLine($"{context}: {ex}");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}