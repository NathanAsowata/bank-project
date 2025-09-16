using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DAL.Models
{
    public class Account
    {
        public int AccountID { get; set; } // This is the Account Number
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string County { get; set; } // Gotten from the Counties Enum
        public string AccountType { get; set; } // Current, Savings
        public int SortCode { get; set; }
        public decimal Balance { get; set; }
        public decimal OverdraftLimit { get; set; }
        public DateTime DateCreated { get; set; }

        // Read-only property useful for display
        public decimal AvailableBalance => Balance + (AccountType == "Current" ? OverdraftLimit : 0);
        public string FullName => $"{FirstName} {Surname}";
        public string DisplayInfo => $"{AccountID} - {FullName} ({AccountType})";
    }
}
