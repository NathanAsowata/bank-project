using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enums;

namespace Shared.Models
{
    public class TransactionLog : ObservableObject // Class name maps to BankTransaction DB Table
    {
        // Private backing fields
        private int _transactionID;
        private int? _sourceAccountID;
        private int? _destinationAccountID;
        private string _destinationSortCode;
        private string _destinationAccountNumber;
        private TransactionType _transactionType;
        private decimal _amount;
        private DateTime _transactionDate;
        private string _reference;
        private int _performingStaffID;
        private TransactionStatus _status;
        private string _notes;

        // Fields populated from the DAO data
        private string _sourceAccountNumber;
        private string _destAccountNumberDisplay;
        private string _performingStaffName;


        // Public Properties
        public int TransactionID
        {
            get => _transactionID;
            set { _transactionID = value; OnPropertyChanged(); }
        }
        public int? SourceAccountID
        {
            get => _sourceAccountID;
            set { _sourceAccountID = value; OnPropertyChanged(); }
        }
        public int? DestinationAccountID
        {
            get => _destinationAccountID;
            set { _destinationAccountID = value; OnPropertyChanged(); }
        }
        public string DestinationSortCode
        { // External sort code from other banks
            get => _destinationSortCode;
            set { _destinationSortCode = value; OnPropertyChanged(); OnPropertyChanged(nameof(DestAccountNumberDisplay)); }
        }
        public string DestinationAccountNumber
        { // External account number from other banks
            get => _destinationAccountNumber;
            set { _destinationAccountNumber = value; OnPropertyChanged(); OnPropertyChanged(nameof(DestAccountNumberDisplay)); }
        }
        public TransactionType TransactionType
        {
            get => _transactionType;
            set { _transactionType = value; OnPropertyChanged(); OnPropertyChanged(nameof(AmountDisplayWithSign)); }
        }
        public decimal Amount
        {
            get => _amount;
            set { _amount = value; OnPropertyChanged(); OnPropertyChanged(nameof(AmountDisplay)); OnPropertyChanged(nameof(AmountDisplayWithSign)); }
        }
        public DateTime TransactionDate
        {
            get => _transactionDate;
            set { _transactionDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(TransactionDateDisplay)); }
        }
        public string Reference
        {
            get => _reference;
            set { _reference = value; OnPropertyChanged(); }
        }
        public int PerformingStaffID
        {
            get => _performingStaffID;
            set { _performingStaffID = value; OnPropertyChanged(); }
        }
        public TransactionStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }
        public string Notes
        {
            get => _notes;
            set { _notes = value; OnPropertyChanged(); }
        }

        // Properties populated by DAO/Service classes from SQL table joins
        public string SourceAccountNumber
        {
            get => _sourceAccountNumber;
            set { _sourceAccountNumber = value; OnPropertyChanged(); }
        }
        public string DestAccountNumberDisplay
        { // Holds internal number OR external sort/number
            get => _destAccountNumberDisplay;
            set { _destAccountNumberDisplay = value; OnPropertyChanged(); }
        }
        public string PerformingStaffName
        {
            get => _performingStaffName;
            set { _performingStaffName = value; OnPropertyChanged(); }
        }


        // Read only values for display on the UI
        public string AmountDisplay => $"{Amount:C}"; // Standard currency format
        public string AmountDisplayWithSign
        { // Show + for Deposit, - for Withdrawal/Transfer
            get
            {
                switch (TransactionType)
                {
                    case TransactionType.Deposit:
                        return $"+{Amount:N2}";
                    case TransactionType.Withdrawal:
                    case TransactionType.InternalTransfer:
                    case TransactionType.ExternalTransfer:
                    case TransactionType.Fee: // Transaction Fees are debits
                        return $"-{Amount:N2}";
                    case TransactionType.Interest: // Interest is credit
                        return $"+{Amount:N2}";
                    default:
                        return $"{Amount:N2}";
                }
            }
        }
        public string TransactionDateDisplay => TransactionDate.ToString("g"); // General short date/time format
    }
}