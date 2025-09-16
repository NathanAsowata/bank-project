using System;
using Shared.Enums;

namespace Shared.Models
{
    public class Account : ObservableObject
    {
        // Private backing fields
        private int _accountID;
        private string _accountNumber;
        private string _sortCode;
        private AccountType _accountType;
        private string _firstName;
        private string _surname;
        private string _email;
        private string _phone;
        private string _addressLine1;
        private string _addressLine2;
        private string _city;
        private County _county;
        private decimal _balance;
        private decimal _overdraftLimit;
        private DateTime _dateOpened;

        // Getters and setters for each property that also includes OnPropertyChanged in setters
        public int AccountID
        {
            get => _accountID;
            set
            {
                _accountID = value;
                OnPropertyChanged();
            }
        }
        public string AccountNumber
        {
            get => _accountNumber;
            set
            {
                _accountNumber = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AccountDisplay)); // Update computed property
            }
        }
        public string SortCode
        {
            get => _sortCode;
            set
            {
                _sortCode = value;
                OnPropertyChanged();
            }
        }
        public AccountType AccountType
        {
            get => _accountType;
            set
            {
                _accountType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AccountDisplay)); // Update computed property
            }
        }
        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullName)); // Update computed property
            }
        }
        public string Surname
        {
            get => _surname;
            set
            {
                _surname = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FullName)); // Update computed property
            }
        }
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }
        public string Phone
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged();
            }
        }
        public string AddressLine1
        {
            get => _addressLine1;
            set
            {
                _addressLine1 = value;
                OnPropertyChanged();
            }
        }
        public string AddressLine2
        {
            get => _addressLine2;
            set
            {
                _addressLine2 = value;
                OnPropertyChanged();
            }
        }
        public string City
        {
            get => _city;
            set
            {
                _city = value;
                OnPropertyChanged();
            }
        }
        public County County
        {
            get => _county;
            set
            {
                _county = value;
                OnPropertyChanged();
            }
        }
        public decimal Balance
        {
            get => _balance;
            set
            {
                _balance = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BalanceDisplay)); // Update computed property
            }
        }
        public decimal OverdraftLimit
        {
            get => _overdraftLimit;
            set
            {
                _overdraftLimit = value;
                OnPropertyChanged();
            }
        }
        public DateTime DateOpened
        {
            get => _dateOpened;
            set
            {
                _dateOpened = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DateOpenedDisplay)); // Update computed property
            }
        }

        // These read-only strings are meant for the UI
        public string FullName => $"{FirstName} {Surname}";
        public string AccountDisplay => $"{AccountNumber} ({AccountType})";
        public string BalanceDisplay => $"{Balance:C}"; // Display balance in currency format
        public string DateOpenedDisplay => DateOpened.ToString("d"); // Short date format
    }
}