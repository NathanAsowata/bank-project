using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class TransactionLog
    {
        public int TransactionID { get; set; }
        public int? SourceAccountID { get; set; }
        public int? DestinationAccountID { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string Description { get; set; }
        public bool RequiresApproval { get; set; }
        public string ApprovalStatus { get; set; } // Pending, Approved, Rejected
        public int? ApprovingManagerID { get; set; }
        public string ReferenceNumber { get; set; }

        // Properties for display in approval data grid view
        public string SourceAccountName { get; set; }
        public string DestinationAccountName { get; set; }


        // Display Transaction Type
        public string DisplayTransactionType
        {
            get
            {
                if (TransactionType == "Transfer")
                {
                    // If DestinationAccountID has a value, it's internal
                    return DestinationAccountID.HasValue ? "Transfer (Internal)" : "Transfer (External)";
                }
                // Otherwise, return the original type (Deposit, Withdrawal, etc.)
                return TransactionType;
            }
        }

        // Display Destination Account Info
        public string DisplayDestination
        {
            get
            {
                // If DestinationAccountID has a value, show it (Internal Transfer)
                if (DestinationAccountID.HasValue)
                {
                    return DestinationAccountID.Value.ToString();
                }
                // If it's an external transfer (Type is Transfer, Dest ID is null)
                else if (TransactionType == "Transfer")
                {
                    // Attempt to parse Sort Code from Description (as added by BIZ layer)
                    // This relies on the BIZ layer's format: "description text... to Sort Code XXXXXX"
                    if (!string.IsNullOrEmpty(Description) && Description.Contains(" to Sort Code "))
                    {
                        var parts = Description.Split(new[] { " to Sort Code " }, StringSplitOptions.None);
                        if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            // We don't have the full external number easily, just show Sort Code hint
                            return $"Ext ({parts[1]})";
                        }
                    }
                    // Fallback if parsing fails or description format is different
                    return "External";
                }
                // For Deposits/Withdrawals, there's no 'destination' in this context
                return "N/A";
            }
        }


    }
}


