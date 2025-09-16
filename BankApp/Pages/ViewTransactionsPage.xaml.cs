using BIZ;
using DAL.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace BankApp.Pages
{
    public partial class ViewTransactionsPage : Page, INotifyPropertyChanged
    {
        private MainWindow _mainWindow;
        private TransactionService _transactionService = new TransactionService();
        private AccountService _accountService = new AccountService();
        private int _accountId;

        private ObservableCollection<TransactionLog> _transactions;
        public ObservableCollection<TransactionLog> Transactions
        {
            get { return _transactions; }
            set { _transactions = value; OnPropertyChanged(nameof(Transactions)); }
        }

        public ViewTransactionsPage(MainWindow mainWindow, int accountId)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _accountId = accountId;
            DataContext = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAccountInfo();
            LoadTransactions();
        }

        private void LoadAccountInfo()
        {
            try
            {
                Account targetAccount = _accountService.GetAccountById(_accountId, App.CurrentUser);
                if (targetAccount == null) return;

                TbAccountNumber.Text = targetAccount.AccountID.ToString();
                TbAccountHolder.Text = targetAccount.FullName;
                TbCurrentBalance.Text = targetAccount.Balance.ToString("C", CultureInfo.GetCultureInfo("en-IE"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading account header info for {_accountId}: {ex.Message}");
                _mainWindow.ShowStatusMessage($"Could not load account header info: {ex.Message}", true);
            }
        }

        private void LoadTransactions()
        {
            try
            {
                List<TransactionLog> transactionList = _transactionService.GetTransactionsForAccount(_accountId, App.CurrentUser);
                Transactions = new ObservableCollection<TransactionLog>(transactionList);
                _mainWindow.ShowStatusMessage($"Loaded {Transactions.Count} transactions for account {_accountId}.");
            }
            catch (Exception ex)
            {
                HandleError("Error loading transactions", ex);
                Transactions = new ObservableCollection<TransactionLog>();
            }
        }

        // UPDATED Back Button Handler Name and Logic
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}